namespace Sirstrap.Core.Windows
{
    public interface IUacService
    {
        bool EnsureAdministratorPrivileges(Func<bool> operation, string[] arguments, string operationDescription);

        bool IsRunningAsAdministrator();

        bool RestartAsAdministrator(string[] arguments);
    }
}
