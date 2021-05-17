using System.Collections.Generic;
using System.Linq;

namespace IcpcResolver.Net.UserControl
{
    public class TeamDto
    {
        public int TeamId;
        
        /// <summary>
        /// display name
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// team name
        /// </summary>
        public string TeamName;

        /// <summary>
        /// school name
        /// </summary>
        public string SchoolName;

        // Problem status before freeze
        public List<ProblemDto> ProblemsFrom;
        // Problem status after contest
        public List<ProblemDto> ProblemsTo;
        // penalty time
        public int PenaltyTime;
        
        /// <summary>
        /// award list, each award in Awards is: AwardName|AwardType, where AwardType is `normal` or `medalist`
        /// </summary>
        public List<string> Awards;

        public int Solved => ProblemsFrom.Count(p => p.IsAccepted);
        public int TimeAll => ProblemsFrom.Sum(p => p.TimeAll(PenaltyTime));

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
                    case false when ProblemsTo[i].Status == ProblemStatus.Pending:
                        ProblemsFrom[i].Status = ProblemStatus.NotTried;
                        ProblemsFrom[i].Time = 0;
                        ProblemsFrom[i].Try = 0;
                        ProblemsTo[i].Status = ProblemStatus.NotTried;
                        ProblemsTo[i].Time = 0;
                        ProblemsTo[i].Try = 0;
                        break;
                }
            }
            return this;
        }
    }
}