using DIM_Kinect7.Drawing;
using DIM_Kinect7.Kinect;
using DIM_Kinect7.Kinect.Consumers;
using DIM_Kinect7.Kinect.Processors;
using DIM_Kinect7.Model;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DIM_Kinect7
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string StatusBarText {
            get => statusBarText;
            private set
            {
                if (statusBarText != value)
                {
                    statusBarText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusBarText)));
                }
            }
        }
        string statusBarText;

        const string GestureDatabasePath = @"data\Kinect7.gbd";

        readonly KinectSensor sensor;
        readonly MultiSourceFrameReader frameReader;

        readonly WpfColorFrameAdapter colorFrameAdapter;
        readonly GameCanvas canvas;

        readonly GestureDetector gestureDetector;

        readonly GameState gameState;

        public MainWindow()
        {
            DataContext = this;

            gameState = new GameState();

            sensor = KinectSensor.GetDefault();
            if (!sensor.IsAvailable)
            {
                StatusBarText = Properties.Resources.NoSensorStatusText;
            }

            frameReader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Body);
            frameReader.MultiSourceFrameArrived += (_sender, _args) => canvas.Render();

            var colorFrameConsumer = new ColorFrameConsumer(frameReader, ColorImageFormat.Bgra, 4);
            //colorFrameAdapter = new WpfColorFrameAdapter(colorFrameConsumer, PixelFormats.Bgra32);

            var depthFrameConsumer = new DepthFrameConsumer(frameReader);
            var depthFrameColorMapper = new DataToGrayScalePixelMapper(depthFrameConsumer);
            colorFrameAdapter = new WpfColorFrameAdapter(depthFrameColorMapper, PixelFormats.Bgra32);

            var frameDescription = sensor.DepthFrameSource.FrameDescription;
            canvas = new GameCanvas(gameState, new DrawingGroup(), frameDescription.Width, frameDescription.Height);

            using (var database = new VisualGestureBuilderDatabase(GestureDatabasePath))
            {
                gestureDetector = new GestureDetector(sensor, database.AvailableGestures.ToArray());
            }
            gestureDetector.GestureDetected += GestureDetector_GestureDetected;

            InitializeComponent();
        }

        void GestureDetector_GestureDetected(GestureDetector.GestureDetectionResult detection)
        {
            if (detection.FirstFrameDetected)
            {
                StatusBarText = $"{detection.GestureName} (C: {detection.Confidence})";
                gameState.CheckCut(CutGestureAssociation.CutFromGesture(detection.GestureName));
            }
        }

        void Window_Loaded(object _sender, RoutedEventArgs _args)
        {
            canvasOverlayImage.Source = canvas.ImageSource;
            sensorFeedbackImage.Source = colorFrameAdapter.ImageSource;

            sensor.Open();
            gameState.Timer.Start();
        }

        void Window_Closing(object _sender, CancelEventArgs _args)
        {
            colorFrameAdapter?.Dispose();
            frameReader?.Dispose();

            sensor?.Close();
            gameState.Timer.Stop();
        }
    }
}
