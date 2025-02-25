namespace Sirstrap.Core
{
    /// <summary>
    /// Provides utility methods for managing file system paths related to
    /// Roblox version installations within the Sirstrap environment.
    /// </summary>
    public static class PathManager
    {
        /// <summary>
        /// Generates the full installation path for a specific Roblox version.
        /// </summary>
        /// <param name="version">The version identifier string.</param>
        /// <returns>
        /// A fully qualified path where the specified version should be installed,
        /// located in the user's LocalApplicationData folder under Sirstrap/Versions/{version}.
        /// </returns>
        /// <remarks>
        /// This method creates a standardized path structure that segregates different
        /// Roblox versions, allowing multiple versions to coexist on the same system.
        /// The base path typically resolves to:
        /// - Windows: C:\Users\{username}\AppData\Local\Sirstrap\Versions\{version}
        /// - macOS: /Users/{username}/Library/Application Support/Sirstrap/Versions/{version}
        /// - Linux: /home/{username}/.local/share/Sirstrap/Versions/{version}
        /// </remarks>
        public static string GetVersionInstallPath(string version)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Versions", version);
        }
    }
}