namespace Sirstrap.Core.Interfaces
{
    public interface IAdministratorService
    {
        public bool Handle(Func<bool> op, string[] args, string opDescription);
    }
}
