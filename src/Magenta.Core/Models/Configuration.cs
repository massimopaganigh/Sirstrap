namespace Magenta.Core.Models
{
    public static class Configuration
    {
        public static int Delay { get; set; } = 5000;

        public static int PlaceId { get; set; } = -1;

        public static List<string> RoblosecurityCookies { get; set; } = [];

        public static int Threads { get; set; } = 1;
    }
}
