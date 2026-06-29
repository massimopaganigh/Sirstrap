using System;
using System.Collections.Generic;

namespace Monsoon.Services.Roblox
{
    public static class RobloxDeployment
    {
        public const string DefaultChannel = "production";

        public const string LauncherEntry = "RobloxPlayerLauncher.exe";

        public const string PackageManifestFile = "rbxPkgManifest.txt";

        public static readonly IReadOnlyList<string> Mirrors =
        [
            "setup.rbxcdn.com",
            "setup-aws.rbxcdn.com",
            "setup-ak.rbxcdn.com",
            "roblox-setup.cachefly.net",
            "s3.amazonaws.com/setup.roblox.com",
        ];

        public static readonly IReadOnlyDictionary<string, string> PlayerPackageDirectories =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["RobloxApp.zip"] = string.Empty,
                ["WebView2.zip"] = string.Empty,
                ["WebView2RuntimeInstaller.zip"] = "WebView2RuntimeInstaller/",
                ["shaders.zip"] = "shaders/",
                ["ssl.zip"] = "ssl/",
                ["content-avatar.zip"] = "content/avatar/",
                ["content-configs.zip"] = "content/configs/",
                ["content-fonts.zip"] = "content/fonts/",
                ["content-sky.zip"] = "content/sky/",
                ["content-sounds.zip"] = "content/sounds/",
                ["content-models.zip"] = "content/models/",
                ["content-textures2.zip"] = "content/textures/",
                ["content-platform-fonts.zip"] = "PlatformContent/pc/fonts/",
                ["content-platform-dictionaries.zip"] = "PlatformContent/pc/shared_compression_dictionaries/",
                ["content-terrain.zip"] = "PlatformContent/pc/terrain/",
                ["content-textures3.zip"] = "PlatformContent/pc/textures/",
                ["extracontent-luapackages.zip"] = "ExtraContent/LuaPackages/",
                ["extracontent-translations.zip"] = "ExtraContent/translations/",
                ["extracontent-models.zip"] = "ExtraContent/models/",
                ["extracontent-textures.zip"] = "ExtraContent/textures/",
                ["extracontent-places.zip"] = "ExtraContent/places/",
            };

        public static IEnumerable<string> CandidateUrls(string channel, string versionedFile)
        {
            var isDefault = channel.Equals(DefaultChannel, StringComparison.OrdinalIgnoreCase);

            foreach (var mirror in Mirrors)
                if (isDefault)
                {
                    yield return $"https://{mirror}/channel/common/{versionedFile}";
                    yield return $"https://{mirror}/{versionedFile}";
                }
                else
                    yield return $"https://{mirror}/channel/{channel.ToLowerInvariant()}/{versionedFile}";
        }

        public static string DeploymentFile(string version, string fileName) => $"{version}-{fileName}";
    }
}
