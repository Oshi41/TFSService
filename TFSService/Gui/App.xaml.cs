using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Gui.Helper;
using Gui.Tests;
using Gui.ViewModels.Notifications;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Wiql;
using Condition = System.Windows.Condition;

namespace Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
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
