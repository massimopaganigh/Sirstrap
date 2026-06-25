namespace Sirstrap.Core.Tests.Cleaner
{
    public class FileSystemServiceTests
    {
        private readonly FileSystemService _fs = new();

        [Fact]
        public void DirectoryAndFileQueries_ReflectRealFileSystem()
        {
            using TempDirectory temp = new();
            temp.WriteFile("a.txt", "1");
            temp.WriteFile(Path.Combine("sub", "b.txt"), "2");

            Assert.True(_fs.DirectoryExists(temp.Path));
            Assert.Contains(temp.Combine("a.txt"), _fs.GetFiles(temp.Path));
            Assert.Contains(temp.Combine("sub"), _fs.GetDirectories(temp.Path));
            Assert.Equal(2, _fs.GetFilesRecursive(temp.Path).Count());
            Assert.Equal(2, _fs.GetFileSystemEntries(temp.Path).Count());
        }

        [Fact]
        public void ClearReadOnlyAttribute_RemovesReadOnlyFlag()
        {
            using TempDirectory temp = new();
            string file = temp.WriteFile("ro.txt", "x");
            File.SetAttributes(file, FileAttributes.ReadOnly);

            _fs.ClearReadOnlyAttribute(file);

            Assert.False(File.GetAttributes(file).HasFlag(FileAttributes.ReadOnly));
        }

        [Fact]
        public void ClearReadOnlyAttribute_IsNoop_WhenNotReadOnly()
        {
            using TempDirectory temp = new();
            string file = temp.WriteFile("rw.txt", "x");

            Assert.Null(Record.Exception(() => _fs.ClearReadOnlyAttribute(file)));
        }

        [Fact]
        public void DeleteFileAndDirectory_RemoveEntries()
        {
            using TempDirectory temp = new();
            string file = temp.WriteFile("f.txt", "x");
            string dir = temp.Combine("d");
            Directory.CreateDirectory(dir);

            _fs.DeleteFile(file);
            _fs.DeleteDirectory(dir, recursive: true);

            Assert.False(File.Exists(file));
            Assert.False(Directory.Exists(dir));
        }
    }
}
