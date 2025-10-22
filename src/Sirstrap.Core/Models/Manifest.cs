namespace Sirstrap.Core.Models
{
    public class Manifest
    {
        public bool IsValid { get; set; } = false;

        public List<string> Packages { get; set; } = [];
    }
}
