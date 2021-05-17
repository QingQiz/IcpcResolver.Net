using System.Diagnostics;
using System.Windows.Media;

namespace IcpcResolver.AppConstants
{
    public static class Colors
    {
        public const string Green = "#0BA70B";
        public const string Red = "#A20000";
        public const string Yellow = "#9C9C00";
        public const string DarkGreen = "#0B4F0B";
        public const string White = "#FFFFFF";
        public const string LightYellow = "#FEFF00";

        public const string Black = "#000000";

        public const string Blue = "#4B82E0";
        
        // DarkGray > BgGray > Gray
        public const string Gray = "#6F6F6F";
        public const string DarkGray = "#282828";
        public const string BgGray = "#3C3C3C";

        public static Color FromString(string color)
        {
            var res = (Color?) new ColorConverter().ConvertFrom(color);
            
            Debug.Assert(res != null);

            return (Color)res;
        }
    }
}