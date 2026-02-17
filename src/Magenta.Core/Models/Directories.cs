namespace Magenta.Core.Models
{
    public static class Directories
    {
        public static string LocalApplicationData { get; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static string MagentaDirectory { get; } = Path.Combine(LocalApplicationData, "Magenta");
    }
}
