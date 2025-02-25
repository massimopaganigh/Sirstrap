namespace Sirstrap.Core
{
    /// <summary>
    /// Provides functionality to parse manifest content from Roblox deployment files
    /// and extract information about required packages for installation.
    /// </summary>
    public static class ManifestParser
    {
        /// <summary>
        /// Parses the raw manifest content string and converts it into a structured Manifest object.
        /// </summary>
        /// <param name="manifestContext">The raw string content of the manifest file.</param>
        /// <returns>
        /// A Manifest object containing the validation status and list of packages 
        /// extracted from the manifest content.
        /// </returns>
        /// <remarks>
        /// The manifest is split into lines, validated for proper format, and 
        /// parsed to extract package filenames.
        /// </remarks>
        public static Manifest Parse(string manifestContext)
        {
            var lines = manifestContext.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

            return new Manifest
            {
                IsValid = IsValidManifest(lines),
                Packages = GetPackages(lines)
            };
        }

        /// <summary>
        /// Determines if the manifest content is valid by checking for the required version marker.
        /// </summary>
        /// <param name="lines">An array of strings representing the lines of the manifest file.</param>
        /// <returns>
        /// <c>true</c> if the manifest contains at least one line and begins with the "v0" version marker;
        /// otherwise, <c>false</c>.
        /// </returns>
        private static bool IsValidManifest(string[] lines)
        {
            return lines.Length > 0 && lines[0].Trim().Equals("v0", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Extracts package filenames from the manifest content.
        /// </summary>
        /// <param name="lines">An array of strings representing the lines of the manifest file.</param>
        /// <returns>
        /// A list of package filenames that are identified by containing a dot (.) and
        /// ending with ".zip" (case insensitive).
        /// </returns>
        private static List<string> GetPackages(string[] lines)
        {
            return [.. lines.Where(line => line.Contains('.') && line.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)).Select(line => line.Trim())];
        }
    }
}