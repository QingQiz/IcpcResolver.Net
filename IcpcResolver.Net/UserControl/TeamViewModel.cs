using System.Collections.Generic;

namespace IcpcResolver.Net.UserControl
{
    public class TeamViewModel
    {
        public int Rank;
        public string Name;
        public IEnumerable<ProblemViewModel> Problems;
    }
}