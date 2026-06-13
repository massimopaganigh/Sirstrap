namespace Sirstrap.Core.Tests.Cleaner
{
    public class FolderDeleterTests
    {
        [Fact]
        public void DeleteFolder_IsNoop_WhenMissing()
        {
            FakeFileSystem fs = new();
            FolderDeleter deleter = new(fs);

            deleter.DeleteFolder(@"C:\missing");

            Assert.Empty(fs.DeletedDirectories);
        }

        [Fact]
        public void DeleteFolder_DeletesExistingDirectory()
        {
            FakeFileSystem fs = new();
            fs.AddDirectory(@"C:\target");
            FolderDeleter deleter = new(fs);

            deleter.DeleteFolder(@"C:\target");

            Assert.Contains(@"C:\target", fs.DeletedDirectories);
        }

        [Fact]
        public void DeleteFolder_DoesNotThrow_WhenForceDeleteAlsoFails()
        {
            FakeFileSystem fs = new();
            fs.AddDirectory(@"C:\stuck");
            fs.AddFile(@"C:\stuck\file.bin");
            fs.DeleteDirectoryThrows[@"C:\stuck"] = () => true;

            FolderDeleter deleter = new(fs);

            Assert.Null(Record.Exception(() => deleter.DeleteFolder(@"C:\stuck")));
            Assert.DoesNotContain(@"C:\stuck", fs.DeletedDirectories);
        }

        [Fact]
        public void DeleteFolder_ClearsReadOnlyAndRetries_WhenAccessDenied()
        {
            FakeFileSystem fs = new();
            fs.AddDirectory(@"C:\locked");
            fs.AddFile(@"C:\locked\readonly.bin");

            int throwCount = 0;
            fs.DeleteDirectoryThrows[@"C:\locked"] = () => throwCount++ == 0;

            FolderDeleter deleter = new(fs);

            deleter.DeleteFolder(@"C:\locked");

            Assert.Contains(@"C:\locked\readonly.bin", fs.ReadOnlyCleared);
            Assert.Contains(@"C:\locked", fs.DeletedDirectories);
        }
    }
}
