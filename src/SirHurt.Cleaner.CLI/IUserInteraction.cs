using System;
using System.Linq;

namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Interface for user interaction
    /// </summary>
    public interface IUserInteraction
    {
        bool ConfirmAction(string message);
    }
}