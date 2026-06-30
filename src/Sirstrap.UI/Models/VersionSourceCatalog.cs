namespace Sirstrap.UI.Models
{
    public static class VersionSourceCatalog
    {
        public static async Task<IReadOnlyList<VersionSourceOption>> BuildAsync(IWeaoService weaoService)
        {
            var options = new List<VersionSourceOption>
            {
                new("SirHurt API", RobloxVersionSources.SirHurt),
                new("Roblox API", RobloxVersionSources.Roblox)
            };

            try
            {
                var versions = await weaoService.GetWindowsVersionsAsync();

                if (!string.IsNullOrWhiteSpace(versions.Current))
                    options.Add(new($"WEAO API: current ({versions.Current})", RobloxVersionSources.Weao));

                if (!string.IsNullOrWhiteSpace(versions.Past))
                    options.Add(new($"WEAO API: previous ({versions.Past})", RobloxVersionSources.VersionPrefix + versions.Past));

                if (!string.IsNullOrWhiteSpace(versions.Future)
                    && !string.IsNullOrWhiteSpace(versions.Current)
                    && versions.Future != versions.Current)
                    options.Add(new($"WEAO API: future ({versions.Future})", RobloxVersionSources.VersionPrefix + versions.Future));

                options.AddRange((await weaoService.GetExecutorsAsync())
                    .Select(executor => new VersionSourceOption($"WEAO API: {executor.Title}", RobloxVersionSources.ExecutorPrefix + executor.Title)));
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to build the WEAO version source options.");
            }

            return options;
        }
    }
}
