namespace IcpcResolver.Net.UserControl
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
    }
}