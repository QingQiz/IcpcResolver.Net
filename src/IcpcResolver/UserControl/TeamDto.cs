using System;
using System.Collections.Generic;
using System.Linq;

namespace IcpcResolver.UserControl
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
                if (ProblemsFrom[i].Status == ProblemStatus.Pending || ProblemsTo[i].Status == ProblemStatus.Pending)
                {
                    throw new ArgumentException("Status should not be `Pending` on Invoking `PostInit`");
                }

                if (ProblemsFrom[i].Try > ProblemsTo[i].Try)
                {
                    throw new ArgumentException(
                        $"Try in ProblemFrom ({ProblemsFrom[i].Try}) > Try in ProblemTo ({ProblemsTo[i].Try})");
                }

                if (ProblemsFrom[i].Time > ProblemsTo[i].Time)
                {
                    throw new ArgumentException(
                        $"Time in ProblemFrom ({ProblemsFrom[i].Time}) > Time in ProblemTo ({ProblemsTo[i].Time})");
                }

                // all same, continue
                if (ProblemsTo[i].Equals(ProblemsFrom[i])) continue;

                // ReSharper disable once InvertIf
                if (!ProblemsFrom[i].IsAccepted)
                {
                    // ReSharper disable once InvertIf
                    if (ProblemsTo[i].Try != ProblemsFrom[i].Try || ProblemsTo[i].Time != ProblemsFrom[i].Time)
                    {
                        ProblemsFrom[i].Status = ProblemStatus.Pending;
                        ProblemsFrom[i].Time = ProblemsTo[i].Time;
                        ProblemsFrom[i].Try = ProblemsTo[i].Try;
                        continue;
                    }
                }
                throw new ArgumentException(
                    $"Invalid status change: {ProblemsFrom[i].Status.ToString()} -> {ProblemsTo[i].Status.ToString()}");
            }
            return this;
        }
    }
}