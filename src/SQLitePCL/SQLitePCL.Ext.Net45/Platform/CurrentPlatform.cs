namespace SQLitePCL
{
    /// <summary>
    /// Implements the <see cref="IPlatform"/> interface for .Net45 Framework.
    /// </summary>
    internal class CurrentPlatform : IPlatform
    {
        /// <summary>
        /// Returns a platform-specific implemention of <see cref="IPlatformMarshal"/>.
        /// </summary>
        IPlatformMarshal IPlatform.PlatformMarshal
        {
            get
            {
                return SQLitePCL.PlatformMarshal.Instance;
            }
        }

        /// <summary>
        /// Returns a platform-specific implemention of <see cref="IPlatformStorage"/>.
        /// </summary>
        IPlatformStorage IPlatform.PlatformStorage
        {
            get
            {
                return SQLitePCL.PlatformStorage.Instance;
            }
        }

        /// <summary>
        /// Returns a platform-specific implemention of <see cref="ISQLite3Provider"/>.
        /// </summary>
        ISQLite3Provider IPlatform.SQLite3Provider
        {
            get
            {
                return SQLitePCL.SQLite3Provider.Instance;
            }
        }
    }
}