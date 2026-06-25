namespace Sirstrap.Core.Launch
{
    public interface IIncognitoManager
    {
        bool MoveRobloxFolderToCache();

        bool RestoreRobloxFolderFromCache();
    }
}
