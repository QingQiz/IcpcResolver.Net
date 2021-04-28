using System.Collections.Generic;
using System.Linq;

namespace IcpcResolver.Net.UserControl
{
    public class TeamDto
    {
        public int Rank;
        public string Name;
        // Problem status before freeze
        public List<ProblemDto> ProblemsFrom;
        // Problem status after contest
        public List<ProblemDto> ProblemsTo;

        public int AcceptedCount => ProblemsFrom.Count(p => p.IsAccepted);
        public int TimeAll => ProblemsFrom.Sum(p => p.TimeAll());

        public TeamDto PostInit()
        {
            for (var i = 0; i < ProblemsTo.Count; i++)
            {
                switch (ProblemsFrom[i].IsAccepted)
                {
                    case true when !ProblemsTo[i].IsAccepted:
                        ProblemsTo[i] = ProblemsFrom[i];
                        break;
                    case false when ProblemsTo[i].IsAccepted:
                    case false when ProblemsTo[i].Status == ProblemStatus.UnAccept:
                        ProblemsFrom[i].Status = ProblemStatus.Pending;
                        ProblemsFrom[i].Time = ProblemsTo[i].Time;
                        ProblemsFrom[i].Try = ProblemsTo[i].Try;
                        break;
                }
            }
            return this;
        }
    }
}