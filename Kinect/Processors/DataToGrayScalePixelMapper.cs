using Microsoft.Kinect;
using System;
using System.Windows.Media;

namespace DIM_Kinect7.Kinect.Processors
{
    class DataToGrayScalePixelMapper : IFrameProcessor<byte[]>
    {
        public FrameDescription FrameDescription => source.FrameDescription;

        public event Action<byte[]> FrameProcessed;

        readonly IFrameProcessor<(ushort[], ushort, ushort)> source;

        readonly PixelWriter pixelWriter;
        delegate void PixelWriter(ref int index, byte red, byte green, byte blue, byte alpha);

        byte[] pixels;

        public DataToGrayScalePixelMapper(IFrameProcessor<(ushort[],ushort,ushort)> source)
            : this(source, PixelFormats.Bgra32)
        { }

        public DataToGrayScalePixelMapper(IFrameProcessor<(ushort[], ushort, ushort)> source, PixelFormat pixelFormat)
        {
            this.source = source;

            if (pixelFormat == PixelFormats.Bgra32)
            {
                pixels = new byte[FrameDescription.Width * FrameDescription.Height * 4];
                pixelWriter = WritePixelBgra32;
            }
            else
            {
                throw new ArgumentException("Pixel format not supported", nameof(pixelFormat));
            }

            source.FrameProcessed += Source_FrameProcessed;
        }

        void Source_FrameProcessed((ushort[], ushort, ushort) args)
        {
            OnFrameArrived(args.Item1, args.Item2, args.Item3);
        }

        void OnFrameArrived(ushort[] data, ushort min, ushort max)
        {
            int pixelIndex = 0;

            for (int i = 0; i < data.Length; i++)
            {
                var datum = data[i];
                byte intensity;
                if (datum == 0)
                {
                    intensity = 255 / 4;
                }
                else if (datum <= min)
                {
                    intensity = 255;
                } 
                else if (datum >= max)
                {
                    intensity = 0;
                }
                else
                {
                    float proportion = (datum - min) / (float)(max - min);
                    intensity = (byte) (255 - 255 * proportion);
                }

                pixelWriter(ref pixelIndex, intensity, intensity, intensity, 255);
            }

            FrameProcessed?.Invoke(pixels);
        }

        void WritePixelBgra32(ref int index, byte red, byte green, byte blue, byte alpha)
        {
            pixels[index++] = blue;     // B
            pixels[index++] = green;    // G
            pixels[index++] = red;      // R
            pixels[index++] = alpha;    // A
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
                source.FrameProcessed -= Source_FrameProcessed;
            }

            pixels = null;
        }
    }
}
