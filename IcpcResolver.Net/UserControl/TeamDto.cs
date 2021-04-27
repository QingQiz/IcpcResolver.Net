using System.Collections.Generic;

namespace IcpcResolver.Net.UserControl
{
    public class TeamDto
    {
        public int Rank;
        public string Name;
        public IEnumerable<ProblemDto> Problems;
    }
}