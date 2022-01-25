using Microsoft.Kinect;
using System;

namespace DIM_Kinect7.Kinect
{
    internal interface IFrameProcessor<R> : IDisposable
    {
        FrameDescription FrameDescription { get; }

        event Action<R> FrameProcessed;
    }
}
