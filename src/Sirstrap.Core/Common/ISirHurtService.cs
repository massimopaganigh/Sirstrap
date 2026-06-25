namespace Sirstrap.Core.Common
{
    public interface ISirHurtService
    {
        string GetSirHurtPath();

        string GetSirHurtUser();

        bool Logout();
    }
}
