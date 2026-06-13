namespace Sirstrap.Core.Cleaner
{
    public sealed class ConsoleUserInteraction(IStatusLine statusLine) : IUserInteraction
    {
        public bool Confirm(string message, bool defaultAnswer = false)
        {
            bool confirmed = defaultAnswer;

            statusLine.InvokeWithStatusHidden(() =>
            {
                Console.WriteLine(message);
                Console.Write(defaultAnswer ? "Do you want to proceed? (Y/n): " : "Do you want to proceed? (y/N): ");

                string? response = Console.ReadLine()?.Trim();

                confirmed = string.IsNullOrEmpty(response)
                    ? defaultAnswer
                    : response.Equals("y", StringComparison.OrdinalIgnoreCase) || response.Equals("yes", StringComparison.OrdinalIgnoreCase);
            });

            return confirmed;
        }
    }
}
