using System;
using System.Linq;

namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Interface for file system operations
    /// </summary>
    public interface IFileSystem
    {
        bool DirectoryExists(string path);
        void DeleteDirectory(string path, bool recursive);
        IEnumerable<string> GetDirectories(string path);
        IEnumerable<string> GetFiles(string path);
        IEnumerable<string> GetFileSystemEntries(string path);
        void DeleteFile(string file);
        bool FileExists(string path);
        string GetRelativePath(string relativeTo, string path);
        string GetDirectoryName(string path);
        string GetFileName(string path);
    }
}