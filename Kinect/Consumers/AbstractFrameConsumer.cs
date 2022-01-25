using Microsoft.Kinect;
using System;

namespace DIM_Kinect7.Kinect.Consumers
{
    public abstract class AbstractFrameConsumer : IDisposable
    {
        protected readonly MultiSourceFrameReader frameReader;

        public AbstractFrameConsumer(MultiSourceFrameReader frameReader)
        {
            this.frameReader = frameReader;
            frameReader.MultiSourceFrameArrived += FrameReader_MultiSourceFrameArrived;
        }

        protected abstract void OnFrameReceived(MultiSourceFrame frame);

        void FrameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs args)
        {
            var frame = args.FrameReference?.AcquireFrame();
            if (frame != null)
            {
                OnFrameReceived(frame);
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
                frameReader.MultiSourceFrameArrived -= FrameReader_MultiSourceFrameArrived;
                frameReader.Dispose();
            }
        }
    }
}
