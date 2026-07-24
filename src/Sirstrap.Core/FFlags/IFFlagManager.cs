namespace Sirstrap.Core.FFlags
{
    public interface IFFlagManager
    {
        string GetFFlagsPath();

        Dictionary<string, object> LoadFFlags();

        void SaveFFlags(Dictionary<string, object> flags);

        void DeployFFlags(string targetDirectory);
    }
}
