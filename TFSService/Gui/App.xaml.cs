using System.Diagnostics;
using System.Windows;
using Gui.Helper;
using Gui.ViewModels.DialogViewModels;
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

            Trace.Listeners.Add(new TextWriterTraceListener(Settings.Settings.Read().LogPath));
            Trace.WriteLine("Starting application");

            RunTests();
        }

        [Conditional("TESTS")]
        private void RunTests()
        {
            var vm = new SettingsViewModel("", null);
            WindowManager.ShowDialog(vm, "Настройки", 450, 500);

            WindowManager.ShowBaloon(new WriteOffBaloonViewModel("Таймер списания времени"));

            //WindowManager.ShowDialog(new TestDialogViewModel(true, true), "Wait for error", width: 300, height: 200);
            //WindowManager.ShowDialog(new TestDialogViewModel(false, false), "No error no awaiting", width: 300, height: 200);
        }
    }
}