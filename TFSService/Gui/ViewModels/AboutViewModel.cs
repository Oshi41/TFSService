using System.Diagnostics;
using System.Windows.Input;
using Gui.Helper;
using Gui.ViewModels.DialogViewModels;
using Mvvm.Commands;
using TfsAPI.Interfaces;

namespace Gui.ViewModels
{
    public class AboutViewModel : BindableExtended
    {
        private readonly IConnect _connect;


        public AboutViewModel(IConnect connect)
        {
            _connect = connect;
            OpenSiteCommand = new ObservableCommand(OpenSite);
        }

        private void OpenSite(object obj)
        {
            if (obj is string url)
            {
                Process.Start(url);
            }
        }

        public ICommand OpenSiteCommand { get; }

        public string Name => _connect?.Name;

        public string Project => _connect?.Project?.Name;
    }
}