namespace SQLitePCL.Ext.WindowsPhone8.Test
{
    using System.Threading;
    using Microsoft.Phone.Controls;
    using Microsoft.VisualStudio.TestPlatform.Core;
    using Microsoft.VisualStudio.TestPlatform.TestExecutor;
    using vstest_executionengine_platformbridge;

    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            this.InitializeComponent();

            var wrapper = new TestExecutorServiceWrapper();
            new Thread(new ServiceMain((param0, param1) => wrapper.SendMessage((ContractName)param0, param1)).Run).Start();
        }
    }
}