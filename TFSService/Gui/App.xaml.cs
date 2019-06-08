using System.Diagnostics;
using System.Windows;
using Gui.Helper;
using Gui.ViewModels.Notifications;

namespace Gui
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            RunTests();
        }

        [Conditional("TESTS")]
        private void RunTests()
        {
            WindowManager.ShowBaloon(new WriteOffBaloonViewModel("Таймер списания времени"));

            //WindowManager.ShowDialog(new TestDialogViewModel(true, true), "Wait for error", width: 300, height: 200);
            //WindowManager.ShowDialog(new TestDialogViewModel(false, false), "No error no awaiting", width: 300, height: 200);
        }
    }
}