namespace SQLitePCL
{
    /// <summary>
    /// An interface for platform-specific assemblies to implement to support 
    /// accessing storage.
    /// </summary>
    public interface IPlatformStorage
    {
        /// <summary>
        /// Returns a platform-specific local file path.
        /// </summary>
        string GetLocalFilePath(string filename);
    }
}
