using Microsoft.Kinect;
using static Microsoft.Kinect.ColorImageFormat;

namespace DIM_Kinect7.Kinect
{
    public static class PixelFormatExtensions
    {
        public static int BytesPerPixel(this ColorImageFormat format)
        {
            switch (format)
            {
                case Rgba: { return 4; }
                case Yuv: { return 2; }
                case Bgra: { return 4; }
                case Bayer: { return 1; }
                case Yuy2: { return 2; }
            }

            return 0;
        }
    }
}
