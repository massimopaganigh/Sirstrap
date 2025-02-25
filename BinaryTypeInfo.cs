namespace Sirstrap
{
    /// <summary>
    /// Encapsulates metadata about a specific Roblox binary type, including
    /// its associated version file and default blob directory location.
    /// </summary>
    public class BinaryTypeInfo
    {
        /// <summary>
        /// Gets or sets the name of the file containing version information for this binary type.
        /// </summary>
        /// <value>
        /// The filename used to retrieve or store version information for this binary type.
        /// </value>
        public string VersionFile { get; set; }

        /// <summary>
        /// Gets or sets the default content blob directory path for this binary type.
        /// </summary>
        /// <value>
        /// The relative path to the default blob directory where packages for this binary type
        /// are stored in the Roblox CDN.
        /// </value>
        public string DefaultBlobDir { get; set; }
    }
}