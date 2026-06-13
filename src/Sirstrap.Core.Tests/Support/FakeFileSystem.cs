namespace Sirstrap.Core.Tests.Support
{
    public sealed class FakeFileSystem : IFileSystem
    {
        public HashSet<string> Directories { get; } = new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> ReadOnlyCleared { get; } = new(StringComparer.OrdinalIgnoreCase);

        public List<string> DeletedDirectories { get; } = [];

        public List<string> DeletedFiles { get; } = [];

        public Dictionary<string, Func<bool>> DeleteDirectoryThrows { get; } = new(StringComparer.OrdinalIgnoreCase);

        public void AddDirectory(string path) => Directories.Add(Norm(path));

        public void AddFile(string path)
        {
            Files.Add(path);
            Directories.Add(Norm(Path.GetDirectoryName(path) ?? path));
        }

        private static string Norm(string path) => path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        public void ClearReadOnlyAttribute(string path) => ReadOnlyCleared.Add(path);

        public void DeleteDirectory(string path, bool recursive)
        {
            if (DeleteDirectoryThrows.TryGetValue(path, out var shouldThrow) && shouldThrow())
                throw new UnauthorizedAccessException(path);

            DeletedDirectories.Add(path);
            Directories.Remove(path);

            foreach (var file in Files.Where(f => f.StartsWith(path, StringComparison.OrdinalIgnoreCase)).ToList())
                Files.Remove(file);
        }

        public void DeleteFile(string path)
        {
            DeletedFiles.Add(path);
            Files.Remove(path);
        }

        public bool DirectoryExists(string path) => Directories.Contains(Norm(path));

        public IEnumerable<string> GetDirectories(string path)
            => Directories.Where(d => IsImmediateChild(Norm(path), d)).ToList();

        public IEnumerable<string> GetFiles(string path)
            => Files.Where(f => string.Equals(Path.GetDirectoryName(f), Norm(path), StringComparison.OrdinalIgnoreCase)).ToList();

        public IEnumerable<string> GetFilesRecursive(string path)
            => Files.Where(f => f.StartsWith(Norm(path) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)).ToList();

        public IEnumerable<string> GetFileSystemEntries(string path)
            => GetDirectories(path).Concat(GetFiles(path)).ToList();

        private static bool IsImmediateChild(string parent, string candidate)
            => string.Equals(Path.GetDirectoryName(candidate), parent, StringComparison.OrdinalIgnoreCase);
    }
}
