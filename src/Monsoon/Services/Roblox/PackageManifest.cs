using System;
using System.Collections.Generic;
using System.IO;

namespace Monsoon.Services.Roblox
{
    public sealed record Package(string FileName, string Signature, int PackedSize, int Size);

    public sealed class PackageManifest(string version, IReadOnlyList<Package> packages)
    {
        private const string SupportedVersion = "v0";

        public static PackageManifest Parse(string version, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new RobloxManifestException("Package manifest is empty.");

            using var reader = new StringReader(content);

            var header = reader.ReadLine()?.Trim();

            if (header != SupportedVersion)
                throw new RobloxManifestException($"Unsupported manifest format '{header}', expected '{SupportedVersion}'.");

            var packages = new List<Package>();

            while (true)
            {
                var fileName = reader.ReadLine();
                var signature = reader.ReadLine();
                var rawPackedSize = reader.ReadLine();
                var rawSize = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(fileName)
                    || string.IsNullOrWhiteSpace(signature)
                    || string.IsNullOrWhiteSpace(rawPackedSize)
                    || string.IsNullOrWhiteSpace(rawSize))
                    break;

                if (fileName.Equals(RobloxDeployment.LauncherEntry, StringComparison.OrdinalIgnoreCase))
                    break;

                if (!int.TryParse(rawPackedSize, out var packedSize)
                    || !int.TryParse(rawSize, out var size))
                    throw new RobloxManifestException($"Invalid size fields for package '{fileName}'.");

                packages.Add(new Package(fileName.Trim(), signature.Trim(), packedSize, size));
            }

            if (packages.Count == 0)
                throw new RobloxManifestException("Package manifest contains no packages.");

            return new PackageManifest(version, packages);
        }

        public IReadOnlyList<Package> Packages { get; } = packages;

        public long TotalPackedSize
        {
            get
            {
                long total = 0;

                foreach (var package in Packages)
                    total += package.PackedSize;

                return total;
            }
        }

        public string Version { get; } = version;
    }
}
