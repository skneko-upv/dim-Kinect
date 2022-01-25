using Microsoft.Kinect;
using System;

namespace DIM_Kinect7.Kinect.Consumers
{
    public class ColorFrameConsumer : AbstractFrameConsumer, IFrameProcessor<byte[]>
    {
        public FrameDescription FrameDescription { get; }
        public int CopyBufferBytesPerPixel { get; }

        public event Action<byte[]> FrameProcessed;

        byte[] copyBuffer;
        readonly ColorImageFormat imageFormat;

        public ColorFrameConsumer(MultiSourceFrameReader frameReader, ColorImageFormat imageFormat, int copyBufferBytesPerPixel)
            : base(frameReader)
        {
            FrameDescription = frameReader.KinectSensor.ColorFrameSource.FrameDescription;

            copyBuffer = new byte[FrameDescription.Width * FrameDescription.Height * copyBufferBytesPerPixel];

            this.imageFormat = imageFormat;
            CopyBufferBytesPerPixel = copyBufferBytesPerPixel;
        }

        protected override sealed void OnFrameReceived(MultiSourceFrame frame)
        {
            bool copiedFrame = false;

            using (var colorFrame = frame.ColorFrameReference?.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    if (colorFrame.RawColorImageFormat == imageFormat)
                    {
                        colorFrame.CopyRawFrameDataToArray(copyBuffer);
                    }
                    else
                    {
                        colorFrame.CopyConvertedFrameDataToArray(copyBuffer, imageFormat);
                    }

                    copiedFrame = true;
                }
            }

            if (copiedFrame)
            {
                FrameProcessed?.Invoke(copyBuffer);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            copyBuffer = null;
        }
    }
}
