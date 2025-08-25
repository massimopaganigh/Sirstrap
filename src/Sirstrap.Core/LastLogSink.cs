using Sirstrap.Core.Extensions;

namespace Sirstrap.Core
{
    public class LastLogSink : ILogEventSink
    {
        private static readonly HttpClient _httpClient = new();

        private static async Task SendError(LogEvent logEvent)
        {
            try
            {
                var version = SirstrapUpdateService.GetCurrentFullVersion();
                var targetFrameworkName = AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName;
                var creationTime = File.GetCreationTime(AppContext.BaseDirectory);
                var oSVersion = Environment.OSVersion;
                var message = System.Web.HttpUtility.JavaScriptStringEncode(TruncateMessage(logEvent.RenderMessage(), 1024));
                var content = $@"{{
  ""embeds"": [
    {{
      ""description"": ""**Version:** {version}\n**Target framework name:** {targetFrameworkName}\n**Creation time:** {creationTime}\n**OS version:** {oSVersion}\n**Error:** {message}"",
      ""color"": 16711680
    }}
  ],
  ""username"": ""Sirstrap"",
  ""avatar_url"": ""https://media.discordapp.net/attachments/1407697017131765792/1407702581211562105/Sirstrap.png?ex=68a710b1&is=68a5bf31&hm=7ba98960f6177237287f522ccdabbe6a98281ba016cabebaa74b9d1f27e1a0a9&=&format=webp&quality=lossless&width=324&height=324""
}}";
                var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                //var sirstrapApi = SirstrapConfiguration.SirstrapApi.FromBase64();

                //await _httpClient.PostAsync(sirstrapApi, stringContent);
            }
            catch { }
        }

        private static string TruncateMessage(string message, int maxLength)
        {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            if (message.Length <= maxLength)
                return message;

            return string.Concat(message.AsSpan(0, maxLength - 3), "...");
        }

        public void Emit(LogEvent logEvent)
        {
            LastLog = logEvent.RenderMessage();
            LastLogTimestamp = logEvent.Timestamp;
            LastLogLevel = logEvent.Level;

            if (logEvent.Level >= LogEventLevel.Error)
                _ = Task.Run(async () => await SendError(logEvent));
        }

        public static string LastLog { get; private set; } = string.Empty;

        public static LogEventLevel? LastLogLevel { get; private set; }

        public static DateTimeOffset? LastLogTimestamp { get; private set; }
    }
}