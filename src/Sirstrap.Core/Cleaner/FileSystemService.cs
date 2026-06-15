namespace Sirstrap.Core.Cleaner
{
    public sealed class FileSystemService : IFileSystem
    {
        public void ClearReadOnlyAttribute(string path)
        {
            var attributes = File.GetAttributes(path);

            if (attributes.HasFlag(FileAttributes.ReadOnly))
                File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
        }

        public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);

        public void DeleteFile(string path) => File.Delete(path);

        public bool DirectoryExists(string path) => Directory.Exists(path);

        public IEnumerable<string> GetDirectories(string path) => Directory.EnumerateDirectories(path);

        public IEnumerable<string> GetFiles(string path) => Directory.EnumerateFiles(path);

        public IEnumerable<string> GetFilesRecursive(string path) => Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);

        public IEnumerable<string> GetFileSystemEntries(string path) => Directory.EnumerateFileSystemEntries(path);
    }
}
