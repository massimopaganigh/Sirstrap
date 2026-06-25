namespace Sirstrap.Core.Tests.Cleaner
{
    public class SelectiveFolderCleanerTests
    {
        private const string Base = @"C:\profile\AppData\Local\Roblox";

        private static SelectiveFolderCleaner NewCleaner(FakeFileSystem fs, out FakeFolderDeleter deleter, bool confirm = true)
        {
            deleter = new FakeFolderDeleter();

            return new SelectiveFolderCleaner(fs, new FakeUserInteraction(confirm), deleter, new CleanerConfig());
        }

        [Fact]
        public void CleanFolderContents_IsNoop_WhenFolderMissing()
        {
            FakeFileSystem fs = new();
            SelectiveFolderCleaner cleaner = NewCleaner(fs, out var deleter);

            cleaner.CleanFolderContents(Base);

            Assert.Empty(fs.DeletedFiles);
            Assert.Empty(deleter.Deleted);
        }

        [Fact]
        public void CleanFolderContents_DeletesNormalFiles_KeepsExcludedFiles()
        {
            FakeFileSystem fs = new();
            fs.AddDirectory(Base);
            fs.AddFile(Path.Combine(Base, "normal.log"));
            fs.AddFile(Path.Combine(Base, "GlobalBasicSettings_13_Studio.xml"));

            SelectiveFolderCleaner cleaner = NewCleaner(fs, out _);

            cleaner.CleanFolderContents(Base);

            Assert.Contains(Path.Combine(Base, "normal.log"), fs.DeletedFiles);
            Assert.DoesNotContain(Path.Combine(Base, "GlobalBasicSettings_13_Studio.xml"), fs.DeletedFiles);
        }

        [Fact]
        public void CleanFolderContents_KeepsExcludedAndNumericSubfolders_DeletesNormalOnes()
        {
            FakeFileSystem fs = new();
            fs.AddDirectory(Base);
            fs.AddDirectory(Path.Combine(Base, "RobloxStudio"));
            fs.AddDirectory(Path.Combine(Base, "99999"));
            fs.AddDirectory(Path.Combine(Base, "logs"));

            SelectiveFolderCleaner cleaner = NewCleaner(fs, out var deleter);

            cleaner.CleanFolderContents(Base);

            Assert.Contains(Path.Combine(Base, "logs"), deleter.Deleted);
            Assert.DoesNotContain(Path.Combine(Base, "RobloxStudio"), deleter.Deleted);
            Assert.DoesNotContain(Path.Combine(Base, "99999"), deleter.Deleted);
        }

        [Fact]
        public void CleanFolderContents_DeletesProtectedFilesInsideProtectedDir_WithConfirmation()
        {
            FakeFileSystem fs = new();
            fs.AddDirectory(Base);
            fs.AddDirectory(Path.Combine(Base, "sirhui"));
            fs.AddFile(Path.Combine(Base, "sirhui", "sirhurta.dat"));

            SelectiveFolderCleaner cleaner = NewCleaner(fs, out var deleter, confirm: true);

            cleaner.CleanFolderContents(Base);

            Assert.Contains(Path.Combine(Base, "sirhui", "sirhurta.dat"), fs.DeletedFiles);
            Assert.DoesNotContain(Path.Combine(Base, "sirhui"), deleter.Deleted);
        }

        [Fact]
        public void CleanFolderContents_KeepsProtectedFile_WhenConfirmationDeclined()
        {
            FakeFileSystem fs = new();
            fs.AddDirectory(Base);
            fs.AddFile(Path.Combine(Base, "Sirstrap.ini"));

            SelectiveFolderCleaner cleaner = NewCleaner(fs, out _, confirm: false);

            cleaner.CleanFolderContents(Base);

            Assert.DoesNotContain(Path.Combine(Base, "Sirstrap.ini"), fs.DeletedFiles);
        }

        [Fact]
        public void CleanFolderContents_DeletesProtectedFile_WhenConfirmed()
        {
            FakeFileSystem fs = new();
            fs.AddDirectory(Base);
            fs.AddFile(Path.Combine(Base, "Sirstrap.ini"));

            SelectiveFolderCleaner cleaner = NewCleaner(fs, out _, confirm: true);

            cleaner.CleanFolderContents(Base);

            Assert.Contains(Path.Combine(Base, "Sirstrap.ini"), fs.DeletedFiles);
        }
    }
}
