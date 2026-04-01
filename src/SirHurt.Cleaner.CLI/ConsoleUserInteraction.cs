using System;
using System.Linq;

namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Console-based implementation of user interaction
    /// </summary>
    public class ConsoleUserInteraction : IUserInteraction
    {
        public bool ConfirmAction(string message)
        {
            Console.WriteLine(message);
            Console.Write("Do you want to proceed? (y/n): ");

            string? response = Console.ReadLine()?.Trim().ToLower();
            return response == "y" || response == "yes";
        }
    }
}