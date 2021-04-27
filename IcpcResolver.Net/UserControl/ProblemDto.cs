namespace IcpcResolver.Net.UserControl
{
    public class ProblemDto
    {
        public ProblemStatus Status;
        public string Label;
        public int Time;
        public int Try;

        public int GetScore(int penalty = 20)
        {
            return Status == ProblemStatus.Accept ? Time + (Try - 1) * penalty : 0;
        }

        public bool IsAccepted()
        {
            return Status is ProblemStatus.Accept or ProblemStatus.FirstBlood;
        }
    }
}