namespace Sirstrap.Core
{
    public class Manifest
    {
        public bool IsValid { get; set; }

        public List<string> Packages { get; set; } = [];
    }
}
