using System.Diagnostics;
using System.Linq;
using System.Windows;
using Gui.Helper;
using Gui.View;
using Gui.ViewModels.Notifications;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

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
            Trace.WriteLine("\n\n\n*******************************************\nStarting application");

#if TESTS
            RunTests();
#else
            StartProgram();
#endif
        }

        private void StartProgram()
        {
            var window = new MainView();
            window.ShowDialog();

            App.Current.Shutdown(0);
        }

        private void RunTests()
        {
            var id = 80439;
            var api = new TfsAPI.TFS.TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");

            var item = api.FindById(id);
            var assigned = new ItemsAssignedBaloonViewModel(new [] { item}, "Новый элемент был назначен");

            WindowManager.ShowBaloon(assigned);


            var write = new WriteOffBaloonViewModel(new ScheduleWorkArgs(item, 4));
            WindowManager.ShowBaloon(write);

            var items = api.GetMyWorkItems();

            //var response = new NewResponsesBaloonViewModel(items.Where(x => x.IsTypeOf(WorkItemTypes.ReviewResponse)),
            //    items.Where(x => x.IsTypeOf(WorkItemTypes.CodeReview)), api, "Мои проверки кода");

            //WindowManager.ShowBaloon(response);


            //var vm = new SettingsViewModel("", null);
            //WindowManager.ShowDialog(vm, "Настройки", 450, 500);

            //WindowManager.ShowBaloon(new WriteOffBaloonViewModel("Таймер списания времени"));

            //WindowManager.ShowDialog(new TestDialogViewModel(true, true), "Wait for error", width: 300, height: 200);
            //WindowManager.ShowDialog(new TestDialogViewModel(false, false), "No error no awaiting", width: 300, height: 200);
        }
    }
}