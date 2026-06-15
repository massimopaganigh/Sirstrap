namespace Sirstrap.Core.Activity
{
    public static partial class RobloxLogParser
    {
        public static string? ExtractServerIp(string logLine)
        {
            try
            {
                if (logLine.Contains("UDMUX")
                    || logLine.Contains("GameHost"))
                {
                    var match = ServerIpRegex().Match(logLine);

                    if (match.Success)
                        return match.Groups[1].Value;
                }

                if (logLine.Contains("server", StringComparison.OrdinalIgnoreCase)
                    || logLine.Contains("connect", StringComparison.OrdinalIgnoreCase))
                {
                    var match = ServerIpRegex().Match(logLine);

                    if (match.Success
                        && !IsPrivateIp(match.Groups[1].Value))
                        return match.Groups[1].Value;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to extract the server IP from a Roblox log line.");
            }

            return null;
        }

        #region PRIVATE METHODS
        private static bool IsPrivateIp(string ip) => ip.StartsWith("127.") || ip.StartsWith("192.168.") || ip.StartsWith("10.") || IsPrivateIpRange172(ip);

        private static bool IsPrivateIpRange172(string ip)
        {
            if (!ip.StartsWith("172."))
                return false;

            var parts = ip.Split('.');

            return parts.Length >= 2 && int.TryParse(parts[1], out var secondOctet) && secondOctet >= 16 && secondOctet <= 31;
        }

        [GeneratedRegex(@"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b")]
        private static partial Regex ServerIpRegex();
        #endregion
    }
}
