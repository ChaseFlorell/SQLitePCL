namespace NGP.Test.WindowsPhone8
{
    using NGP.Test.WindowsPhone8.Resources;

    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static AppResources localizedResources = new AppResources();

        public AppResources LocalizedResources
        {
            get { return localizedResources; }
        }
    }
}