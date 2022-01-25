using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DIM_Kinect7.Kinect
{
    internal class WpfColorFrameAdapter : IDisposable
    {
        public ImageSource ImageSource => bitmap;

        const double dpiX = 96;
        const double dpiY = 96;

        readonly WriteableBitmap bitmap;
        readonly IFrameProcessor<byte[]> consumer;
        readonly int sourceBytesPerPixel;

        internal WpfColorFrameAdapter(IFrameProcessor<byte[]> consumer, PixelFormat pixelFormat)
        {
            var frameDescription = consumer.FrameDescription;
            bitmap = new WriteableBitmap(frameDescription.Width, frameDescription.Height, dpiX, dpiY, pixelFormat, null);

            this.consumer = consumer;
            consumer.FrameProcessed += Consumer_ColorFrameCopied;
            sourceBytesPerPixel = (int) Math.Ceiling(pixelFormat.BitsPerPixel / 8d);
        }

        void Consumer_ColorFrameCopied(byte[] pixels)
        {
            bitmap.WritePixels(
                new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
                pixels,
                bitmap.PixelWidth * sourceBytesPerPixel,
                0);
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
                consumer.FrameProcessed -= Consumer_ColorFrameCopied;
            }
        }
    }
}
