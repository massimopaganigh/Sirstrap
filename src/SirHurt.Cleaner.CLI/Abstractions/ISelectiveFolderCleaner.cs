namespace SirHurt.Cleaner.CLI.Abstractions
{
    public interface ISelectiveFolderCleaner
    {
        void CleanFolderContents(string folderPath);
    }
}
