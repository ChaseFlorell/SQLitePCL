namespace SQLitePCL.Ext.WindowsPhone8.Test
{
    using SQLitePCL.Ext.WindowsPhone8.Test.Resources;

    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static AppResources localizedResources = new AppResources();

        public AppResources LocalizedResources
        {
            get
            {
                return localizedResources;
            }
        }
    }
}