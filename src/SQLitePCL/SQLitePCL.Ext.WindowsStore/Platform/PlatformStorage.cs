namespace SQLitePCL
{
    using System.IO;
    using Windows.Storage;

    /// <summary>
    /// Implements the <see cref="IPlatformStorage"/> interface for Windows Store.
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
            var result = filename;

            if (!Path.IsPathRooted(filename))
            {
                result = Path.Combine(ApplicationData.Current.LocalFolder.Path, filename);
            }

            return result;
        }
    }
}
