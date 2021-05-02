using System;
using System.Collections.Generic;
using System.Linq;
using IcpcResolver.Net.UserControl;

namespace IcpcResolver.Net.Window
{
    public class ResolverDto
    {
        public ResolverConfig ResolverConfig;

        public List<Team> Teams;

        private static List<TeamDto> DataGenerator(int problemN, int teamN)
        {
            var values = Enum.GetValues(typeof(ProblemStatus));
            var random = new Random();

            ProblemDto GetProblem(int n)
            {
                var status = (ProblemStatus) (values.GetValue(random.Next(values.Length)) ?? ProblemStatus.NotTried);

                return new ProblemDto
                {
                    Label = new string(new[] {(char) ('A' + n)}),
                    Status = status,
                    Time = status == ProblemStatus.NotTried ? 0 : random.Next(1, 300),
                    Try = status == ProblemStatus.NotTried ? 0 : random.Next(1, 5)
                };
            }

            return Enumerable
                .Range(0, teamN)
                .Select(n =>
                {
                    // NOTE there must be `ToList`
                    List<ProblemDto> Problems() =>
                        Enumerable.Range(0, problemN)
                            .Select((Func<int, ProblemDto>) GetProblem)
                            .ToList();

                    return new TeamDto
                    {
                        TeamRank = 0,
                        TeamName = "Team" + n,
                        ProblemsFrom = Problems(),
                        ProblemsTo = Problems()
                    }.PostInit();
                })
                .OrderByDescending(t => t.Solved)
                .ThenBy(t => t.TimeAll)
                .ToList();
        }
    }
}