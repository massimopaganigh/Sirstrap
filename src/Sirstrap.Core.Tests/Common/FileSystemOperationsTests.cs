namespace Sirstrap.Core.Tests.Common
{
    public class FileSystemOperationsTests
    {
        [Fact]
        public void CreateDirectory_CreatesAndIsIdempotent()
        {
            using TempDirectory temp = new();
            string target = temp.Combine("created");

            FileSystemOperations.CreateDirectory(target);
            FileSystemOperations.CreateDirectory(target);

            Assert.True(Directory.Exists(target));
        }

        [Fact]
        public void DeleteDirectory_RemovesDirectory_AndIsNoopWhenMissing()
        {
            using TempDirectory temp = new();
            string target = temp.Combine("to-delete");
            Directory.CreateDirectory(target);

            FileSystemOperations.DeleteDirectory(target);
            Assert.False(Directory.Exists(target));

            FileSystemOperations.DeleteDirectory(target);
        }

        [Fact]
        public void DeleteFile_RemovesFile_AndIsNoopWhenMissing()
        {
            using TempDirectory temp = new();
            string file = temp.WriteFile("file.txt", "content");

            FileSystemOperations.DeleteFile(file);
            Assert.False(File.Exists(file));

            FileSystemOperations.DeleteFile(file);
        }

        [Fact]
        public void MoveDirectory_MovesContents()
        {
            using TempDirectory temp = new();
            string source = temp.Combine("source");
            string destination = temp.Combine("destination");
            Directory.CreateDirectory(source);
            File.WriteAllText(Path.Combine(source, "f.txt"), "x");

            FileSystemOperations.MoveDirectory(source, destination);

            Assert.False(Directory.Exists(source));
            Assert.True(File.Exists(Path.Combine(destination, "f.txt")));
        }

        [Fact]
        public void MoveDirectory_Throws_WhenSourceMissing()
        {
            using TempDirectory temp = new();

            Assert.Throws<InvalidOperationException>(() => FileSystemOperations.MoveDirectory(temp.Combine("missing"), temp.Combine("dest"), attempts: 1));
        }
    }
}
