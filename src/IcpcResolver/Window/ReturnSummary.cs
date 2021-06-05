using System.Collections.Generic;
using System.Linq;

namespace IcpcResolver.Window
{
    public class ReturnSummary
    {
        public bool HasError => ErrList.Any();
        public string ErrType;
        public List<string> ErrList = new();
    }
}