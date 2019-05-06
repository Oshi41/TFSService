using System.Collections.Generic;
using System.Windows;
using Gui.ViewModels;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI;

namespace Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //var tfs = new Tfs("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
            //Content = new WorkItemSearcher(tfs);
        }
    }
}
