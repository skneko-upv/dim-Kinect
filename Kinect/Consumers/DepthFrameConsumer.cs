using Microsoft.Kinect;
using System;

namespace DIM_Kinect7.Kinect.Consumers
{
    public class DepthFrameConsumer : AbstractFrameConsumer, IFrameProcessor<(ushort[], ushort, ushort)>
    {
        public FrameDescription FrameDescription { get; }

        public event Action<(ushort[], ushort, ushort)> FrameProcessed;

        readonly ushort[] copyBuffer;

        public DepthFrameConsumer(MultiSourceFrameReader frameReader)
            : base(frameReader)
        {
            FrameDescription = frameReader.KinectSensor.DepthFrameSource.FrameDescription;

            copyBuffer = new ushort[FrameDescription.Width * FrameDescription.Height];
        }

        protected sealed override void OnFrameReceived(MultiSourceFrame frame)
        {
            bool copiedFrame = false;
            ushort minDepth = 0, maxDepth = 0;

            using (var depthFrame = frame.DepthFrameReference?.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    depthFrame.CopyFrameDataToArray(copyBuffer);
                    minDepth = depthFrame.DepthMinReliableDistance;
                    maxDepth = depthFrame.DepthMaxReliableDistance;

                    copiedFrame = true;
                }
            }

            if (copiedFrame)
            {
                FrameProcessed?.Invoke((copyBuffer, minDepth, maxDepth));
            }
        }
    }
}
