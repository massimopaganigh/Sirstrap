namespace Sirstrap.Core.Update
{
    public sealed class SirstrapUpdateService(
        GitHubReleaseClient releaseClient,
        UpdateApplier updateApplier,
        SirstrapConfiguration sirstrapConfiguration,
        ISirstrapVersion sirstrapVersion,
        IPerformanceTelemetry performanceTelemetry) : ISirstrapUpdateService
    {
        private const string CLI_ZIP_FILENAME = "Sirstrap.CLI.zip";
        private const string UI_ZIP_FILENAME = "Sirstrap.UI.zip";

        public async Task<string> GetLatestChangelogAsync()
        {
            var (_, _, release) = await FindLatestReleaseAsync();

            return release?.Body ?? string.Empty;
        }

        public async Task UpdateAsync(SirstrapType sirstrapType, string[] args)
        {
            using ITelemetryScope scope = performanceTelemetry.Measure("update.check", new Dictionary<string, object>
            {
                ["sirstrapType"] = sirstrapType.ToString()
            });

            UpdateOutcome outcome;

            try
            {
                if (!sirstrapConfiguration.SirstrapAutoUpdate)
                {
                    Log.Information("[*] AutoUpdate is disabled, skipping the update check.");

                    outcome = UpdateOutcome.Disabled;
                }
                else if (await IsUpToDateAsync())
                {
                    outcome = UpdateOutcome.UpToDate;
                }
                else
                {
                    bool applied = await DownloadAndApplyUpdateAsync(sirstrapType, args);

                    outcome = applied ? UpdateOutcome.Updated : UpdateOutcome.Failed;

                    if (!applied)
                        scope.MarkFailed();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to update Sirstrap.");

                scope.MarkFailed();

                outcome = UpdateOutcome.Failed;
            }

            scope.SetTag("outcome", outcome.ToString());

            performanceTelemetry.RecordCounter("update.check.outcome", new Dictionary<string, object>
            {
                ["outcome"] = outcome.ToString(),
                ["sirstrapType"] = sirstrapType.ToString()
            });
        }

        private async Task<bool> DownloadAndApplyUpdateAsync(SirstrapType sirstrapType, string[] args)
        {
            try
            {
                var assetName = GetAssetName(sirstrapType);
                var (_, _, release) = await FindLatestReleaseAsync();
                var downloadUri = release?.FindAssetDownloadUri(assetName) ?? string.Empty;

                if (string.IsNullOrEmpty(downloadUri))
                    throw new InvalidOperationException($"No '{assetName}' asset found in the latest release.");

                await updateApplier.ApplyAsync(downloadUri, args);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to download and apply the Sirstrap update.");

                return false;
            }
        }

        private async Task<(Version Version, string Channel, GitHubRelease? Release)> FindLatestReleaseAsync()
        {
            Version latestVersion = new("0.0.0.0");
            var latestChannel = string.Empty;
            GitHubRelease? latestRelease = null;

            foreach (var release in await releaseClient.GetReleasesAsync())
            {
                if (release.IsDraft
                    || !ReleaseTag.TryParse(release.TagName, out var tag)
                    || !string.Equals(NormalizeChannel(tag.Channel), NormalizeChannel(sirstrapVersion.Channel), StringComparison.OrdinalIgnoreCase))
                    continue;

                if (tag.Version > latestVersion)
                {
                    latestVersion = tag.Version;
                    latestChannel = tag.Channel;
                    latestRelease = release;
                }
            }

            return (latestVersion, latestChannel, latestRelease);
        }

        private static string GetAssetName(SirstrapType sirstrapType) => sirstrapType == SirstrapType.CLI ? CLI_ZIP_FILENAME : UI_ZIP_FILENAME;

        private static string NormalizeChannel(string? channel) => channel?.Trim().TrimStart('-') ?? string.Empty;

        private async Task<bool> IsUpToDateAsync()
        {
            try
            {
                var currentVersion = sirstrapVersion.Current;
                var currentChannel = sirstrapVersion.Channel;
                var (latestVersion, latestChannel, latestRelease) = await FindLatestReleaseAsync();

                if (latestRelease is null)
                {
                    Log.Warning("[*] No Sirstrap release found for the {Channel} channel, skipping the update check.", currentChannel);

                    return true;
                }

                if (latestVersion > currentVersion)
                {
                    Log.Information("[*] Updating Sirstrap from v{CurrentVersion}{CurrentChannel} to v{LatestVersion}{LatestChannel}...", currentVersion, currentChannel, latestVersion, latestChannel);

                    return false;
                }

                Log.Information("[*] Sirstrap is up to date: v{CurrentVersion}{CurrentChannel}.", currentVersion, currentChannel);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to check whether Sirstrap is up to date.");

                return true;
            }
        }
    }
}
