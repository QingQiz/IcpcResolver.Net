using System.Collections.Generic;
using System.Linq;

namespace IcpcResolver.Net.UserControl
{
    public class TeamDto
    {
        public int Rank;
        public string Name;
        public List<ProblemDto> Problems;

        public int AcceptedCount => Problems.Count(p => p.IsAccepted());
        public int TimeAll => Problems.Sum(p => p.GetScore());
    }
}