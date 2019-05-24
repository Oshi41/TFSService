using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Gui.Helper;
using Gui.Tests;
using Gui.ViewModels.Notifications;

namespace Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if TESTS

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            RunTests();
        }
        
        public void RunTests()
        {
            WindowManager.ShowBaloon(new WriteOffBaloonViewModel("Таймер списания времени"));

            // App.Current.Shutdown();

            //WindowManager.ShowDialog(new TestDialogViewModel(true, true), "Wait for error", width: 300, height: 200);

            //WindowManager.ShowDialog(new TestDialogViewModel(false, false), "No error no awaiting", width: 300, height: 200);
        }
#endif

    }
}
