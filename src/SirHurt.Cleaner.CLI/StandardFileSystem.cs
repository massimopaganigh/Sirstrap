using System;
using System.Linq;

namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Standard file system implementation
    /// </summary>
    public class StandardFileSystem : IFileSystem
    {
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);
        public IEnumerable<string> GetDirectories(string path) => Directory.GetDirectories(path);
        public IEnumerable<string> GetFiles(string path) => Directory.GetFiles(path);
        public IEnumerable<string> GetFileSystemEntries(string path) => Directory.EnumerateFileSystemEntries(path);
        public void DeleteFile(string file) => File.Delete(file);
        public bool FileExists(string path) => File.Exists(path);
        public string GetRelativePath(string relativeTo, string path) => Path.GetRelativePath(relativeTo, path);
        public string GetDirectoryName(string path) => Path.GetDirectoryName(path) ?? string.Empty;
        public string GetFileName(string path) => Path.GetFileName(path);
    }
}