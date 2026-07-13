namespace Sirstrap.Core.FastFlags
{
    public interface IFastFlagService
    {
        void Apply(string versionDirectory, string? fastFlagsFilePath = null);

        IReadOnlyDictionary<string, string> GetFlags(string? fastFlagsFilePath = null);

        void SetFlags(IReadOnlyDictionary<string, string> flags, string? fastFlagsFilePath = null);
    }
}
