using System.Linq;
using System.Windows;
using Gui.Helper;
using Gui.ViewModels.DialogViewModels;
using TfsAPI.TFS;

namespace Gui.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            //var tfs = new Tfs("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
            //var searcher = new WorkItemSearcher(tfs);

            //WindowManager.ShowDialog(searcher, "Search", width: 300);
            //App.Current.Shutdown();

            //var vm = new TextInputVm("Введите строку подключения к Team Foundation Server",
            //    s =>
            //    {
            //        if (!Uri.TryCreate(s, UriKind.Absolute, out var result))
            //        {
            //            return "Эта строчка не явялется веб-адресом";
            //        }

            //        return null;
            //    });


            var vm = new CheckinHistoryViewModel(new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"));
            WindowManager.ShowDialog(vm, "Расписание на месяц", width: 500, height:500);

            //var tfs = new Tfs("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
            //tfs.ItemsChanged += (sender, item) => { MessageBox.Show(string.Join(", ", item.Keys.Select(x => x.Id))); };
            //tfs.Start();

            //var vm = new ChooseTaskViewModel(new Tfs("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"));

            //WindowManager.ShowDialog(vm, "Первое подключение", width: 400, height:200);
#endif

        }
    }
}
