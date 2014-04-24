namespace NGP.Test.IOS
{
    using MonoTouch.UIKit;
    using SQLitePCL;

    public class Application
    {
        // This is the main entry point of the application.
        private static void Main(string[] args)
        {
            CurrentPlatform.Init();

            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}