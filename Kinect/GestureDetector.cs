using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Linq;

namespace DIM_Kinect7.Kinect
{
    class GestureDetector : IDisposable
    {
        public struct GestureDetectionResult
        {
            public string GestureName { get; }
            public bool FirstFrameDetected { get; }
            public float Confidence { get; }

            public GestureDetectionResult(string gestureName, bool firstFrameDetected, float confidence)
            {
                GestureName = gestureName;
                FirstFrameDetected = firstFrameDetected;
                Confidence = confidence;
            }
        }

        public event GestureDetectedEventHandler GestureDetected;
        public delegate void GestureDetectedEventHandler(GestureDetectionResult result);

        public event Action TrackingLost;

        public ulong TrackingId
        {
            get => frameSource.TrackingId;
            set
            {
                if (frameSource.TrackingId != value)
                {
                    frameSource.TrackingId = value;
                }
            }
        }

        public bool IsPaused
        {
            get => frameReader.IsPaused;
            set
            {
                if (frameReader.IsPaused != value)
                {
                    frameReader.IsPaused = value;
                }
            }
        }

        readonly VisualGestureBuilderFrameSource frameSource;
        readonly VisualGestureBuilderFrameReader frameReader;

        readonly BodyFrameReader bodyFrameReader;
        Body[] bodies;

        public GestureDetector(KinectSensor sensor, params Gesture[] gestures)
        {
            if (sensor == null)
            {
                throw new ArgumentNullException(nameof(sensor));
            }

            frameSource = new VisualGestureBuilderFrameSource(sensor, 0);
            frameSource.TrackingIdLost += FrameSource_TrackingIdLost;

            frameReader = frameSource.OpenReader();
            if (frameReader != null)
            {
                frameReader.IsPaused = true;
                frameReader.FrameArrived += FrameReader_FrameArrived;
            }

            frameSource.AddGestures(gestures);

            bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            if (bodyFrameReader != null)
            {
                bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
                bodyFrameReader.IsPaused = false;
            }
        }

        private void BodyFrameReader_FrameArrived(object _sender, BodyFrameArrivedEventArgs args)
        {
            bool dataReceived = false;

            using (var bodyFrame = args.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (bodies == null || bodies.Length < bodyFrame.BodyCount)
                    {
                        bodies = new Body[bodyFrame.BodyCount];
                    }
                    bodyFrame.GetAndRefreshBodyData(bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                var trackedBody = bodies.FirstOrDefault(body => body.IsTracked);
                if (trackedBody != null)
                {
                    bodyFrameReader.IsPaused = true;

                    TrackingId = trackedBody.TrackingId;
                    IsPaused = false;
                }
            }
        }

        void FrameSource_TrackingIdLost(object _sender, TrackingIdLostEventArgs _args)
        {
            frameReader.IsPaused = true;
            TrackingLost?.Invoke();
            bodyFrameReader.IsPaused = false;
        }

        void FrameReader_FrameArrived(object _sender, VisualGestureBuilderFrameArrivedEventArgs args)
        {
            GestureDetectionResult? result = null;

            using (var frame = args.FrameReference.AcquireFrame())
            {
                var discreteResults = frame?.DiscreteGestureResults;
                if (discreteResults != null)
                {
                    foreach (var gesture in frameSource.Gestures)
                    {
                        if (gesture.GestureType == GestureType.Discrete && discreteResults.TryGetValue(gesture, out var kinectResult))
                        {
                            if (kinectResult.Detected)
                            {
                                result = new GestureDetectionResult(
                                    gesture.Name,
                                    kinectResult.FirstFrameDetected,
                                    kinectResult.Confidence);
                                break;
                            }
                        }
                        result = null;
                    }
                }
                else
                {
                    result = null;
                }
            }

            if (result.HasValue)
            {
                GestureDetected?.Invoke(result.Value);
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (frameReader != null)
                {
                    frameReader.FrameArrived -= FrameReader_FrameArrived;
                    frameReader.Dispose();
                }

                if (frameSource != null)
                {
                    frameSource.TrackingIdLost -= FrameSource_TrackingIdLost;
                    frameSource.Dispose();
                }
            }
        }
    }
}
