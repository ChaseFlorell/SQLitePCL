namespace SQLitePCL
{
    using System.IO;

    /// <summary>
    /// Implements the <see cref="IPlatformStorage"/> interface for .Net45 Framework.
    /// </summary>
    internal class PlatformStorage : IPlatformStorage
    {
        /// <summary>
        /// A singleton instance of the <see cref="PlatformStorage"/>.
        /// </summary>
        private static IPlatformStorage instance = new PlatformStorage();

        private PlatformStorage()
        {
        }

        /// <summary>
        /// A singleton instance of the <see cref="PlatformStorage"/>.
        /// </summary>
        public static IPlatformStorage Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Returns a platform-specific local file path.
        /// </summary>
        string IPlatformStorage.GetLocalFilePath(string filename)
        {
            return Path.GetFullPath(filename);
        }
    }
}
