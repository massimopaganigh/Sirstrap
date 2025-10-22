namespace Sirstrap.Core
{
    /// <summary>
    /// Defines the type of instance in a multi-instance environment.
    /// </summary>
    public enum InstanceType
    {
        /// <summary>
        /// Instance type not yet determined or multi-instance mode is disabled.
        /// </summary>
        None,

        /// <summary>
        /// Master instance - has captured the Roblox singleton and manages the primary Roblox process.
        /// </summary>
        Master,

        /// <summary>
        /// Slave instance - failed to capture the singleton and operates alongside a master instance.
        /// </summary>
        Slave
    }
}
