using System;
using static DIM_Kinect7.Model.CutKind;

namespace DIM_Kinect7.Model
{
    public enum CutKind
    {
        LineRight,
        LineLeft,
        LineUp,
        CircleClockwise,
        CircleCounterclockwise
    }

    public static class CutGestureAssociation
    {
        public static CutKind CutFromGesture(string gestureName)
        {
            switch (gestureName)
            {
                case "ArmSwipe_Left": return LineRight;     // mirrored on purpose
                case "ArmSwipe_Right": return LineLeft;
                case "ArmRiseUp": return LineUp;
                case "ArmCircle_Clockwise": return CircleClockwise;
                case "ArmCircle_Counterclockwise": return CircleCounterclockwise;
            }

            throw new ArgumentException(nameof(gestureName));
        }
    }
}
