namespace Sirstrap
{
    /// <summary>
    /// Represents a manifest that contains information about Roblox packages 
    /// to be downloaded and installed during the deployment process.
    /// </summary>
    public class Manifest
    {
        /// <summary>
        /// Gets or sets a value indicating whether the manifest data is valid and can be used.
        /// </summary>
        /// <value>
        /// <c>true</c> if the manifest was successfully loaded and contains valid data;
        /// otherwise, <c>false</c>.
        /// </value>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the collection of package filenames that need to be downloaded
        /// and processed as part of the installation.
        /// </summary>
        /// <value>
        /// A list of strings where each string represents a package filename.
        /// </value>
        public List<string> Packages { get; set; }
    }
}