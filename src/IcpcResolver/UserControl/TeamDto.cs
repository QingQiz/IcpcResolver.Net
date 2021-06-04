using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IcpcResolver.UserControl
{
    public class TeamDto
    {
        public int TeamId;

        /// <summary>
        /// team's organization icon
        /// </summary>
        public string IconPath;

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
                var f = ProblemsFrom[i];
                var t = ProblemsTo[i];
                if (f.Status == ProblemStatus.Pending || t.Status == ProblemStatus.Pending)
                {
                    throw new ArgumentException("Status should not be `Pending` on Invoking `PostInit`");
                }

                if (f.Try > t.Try)
                {
                    throw new ArgumentException(
                        $"Try in ProblemFrom ({f.Try}) > Try in ProblemTo ({t.Try})");
                }

                if (f.Time > t.Time)
                {
                    throw new ArgumentException(
                        $"Time in ProblemFrom ({f.Time}) > Time in ProblemTo ({t.Time})");
                }

                // all same, continue
                if (t.Equals(f)) continue;

                // ReSharper disable once InvertIf
                if (!f.IsAccepted)
                {
                    // ReSharper disable once InvertIf
                    if (t.Try != f.Try || t.Time != f.Time)
                    {
                        f.Status = ProblemStatus.Pending;
                        f.Time = t.Time;
                        f.Try = t.Try;
                        continue;
                    }
                }

                throw new ArgumentException(
                    $"Invalid status change: {f.Status.ToString()} ({f.Try}, {f.Time}) -> {t.Status.ToString()} ({t.Try}, {t.Time})");
            }
            return this;
        }
    }
}