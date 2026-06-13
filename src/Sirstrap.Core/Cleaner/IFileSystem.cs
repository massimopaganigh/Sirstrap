namespace Sirstrap.Core.Cleaner
{
    public interface IFileSystem
    {
        void ClearReadOnlyAttribute(string path);

        void DeleteDirectory(string path, bool recursive);

        void DeleteFile(string path);

        bool DirectoryExists(string path);

        IEnumerable<string> GetDirectories(string path);

        IEnumerable<string> GetFiles(string path);

        IEnumerable<string> GetFilesRecursive(string path);

        IEnumerable<string> GetFileSystemEntries(string path);
    }
}
