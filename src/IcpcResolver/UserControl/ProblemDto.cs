using System;
using System.Diagnostics;

namespace IcpcResolver.UserControl
{
    public class ProblemDto
    {
        public ProblemStatus Status;
        public string Label;
        public int Time;
        public int Try;

        public int TimeAll(int penalty = 20)
        {
            return IsAccepted ? Time + (Try - 1) * penalty : 0;
        }

        public bool IsAccepted => Status is ProblemStatus.Accept or ProblemStatus.FirstBlood;

        public override bool Equals(object y)
        {
            var other = y as ProblemDto ?? new ProblemDto();
            return Status == other.Status && Label == other.Label && Time == other.Time && Try == other.Try;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int) Status, Label, Time, Try);
        }
    }
}