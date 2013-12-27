namespace SQLitePCL
{
    /// <summary>
    /// Provides an interface that platform-specific SQLite Wrapper assemblies 
    /// can implement to provide functionality required by the SQLite Wrapper PCL 
    /// that is platform specific.
    /// </summary>
    public interface IPlatform
    {
        /// <summary>
        /// Returns a platform-specific implemention of <see cref="IPlatformMarshal"/>.
        /// </summary>
        IPlatformMarshal PlatformMarshal { get; }

        /// <summary>
        /// Returns a platform-specific implemention of <see cref="IPlatformStorage"/>.
        /// </summary>
        IPlatformStorage PlatformStorage { get; }

        /// <summary>
        /// Returns a platform-specific implemention of <see cref="ISQLite3Provider"/>.
        /// </summary>
        ISQLite3Provider SQLite3Provider { get; }
    }
}
