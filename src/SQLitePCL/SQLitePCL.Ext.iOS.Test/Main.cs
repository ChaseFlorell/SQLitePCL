using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace SQLitePCL.Ext.iOS.Test
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main(string[] args)
		{
// ReSharper disable UnusedVariable
			// init the  buts to stop the linker removing assemblies
			var platform = SQLite3Provider.Instance;
// ReSharper restore UnusedVariable

			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main(args, null, "AppDelegate");
		}
	}
}