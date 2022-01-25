using System;
using System.Diagnostics;

namespace DIM_Kinect7.Model
{
    class GameState
    {
        public CutKind CurrentCut { get; private set; }
        public uint Score { get; private set; }
        public Stopwatch Timer { get; }

        public event Action CutPassed;
        public event Action CutFailed;

        readonly Random rng = new Random();
        readonly Array cutKindValues = Enum.GetValues(typeof(CutKind));

        public GameState()
        {
            NextCut();
            Score = 0;
            Timer = new Stopwatch();
        }

        public bool CheckCut(CutKind cut)
        {
            var match = cut == CurrentCut;
            
            if (match)
            {
                Score++;
                CutPassed();
            }
            else
            {
                CutFailed();
            }

            NextCut();

            return match;
        }

        public void NextCut()
        {
            var randomIndex = rng.Next(cutKindValues.Length);

            if (randomIndex == (int)CurrentCut)  // avoid repetition
            {
                randomIndex = (randomIndex + 1) % cutKindValues.Length;
            }

            CurrentCut = (CutKind)cutKindValues.GetValue(randomIndex);
        }
    }
}
