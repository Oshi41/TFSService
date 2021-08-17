using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Gui.View
{
    public partial class CodeRequestsView : UserControl
    {
        public CodeRequestsView()
        {
            InitializeComponent();
        }

        private void OnHyperlinkClick(object sender, RoutedEventArgs e)
        {
            // todo
            
            // var destination = ((Hyperlink)e.OriginalSource).NavigateUri;
            //
            // using (Process browser = new Process())
            // {
            //     browser.StartInfo = new ProcessStartInfo
            //     {
            //         FileName = destination.ToString(),
            //         UseShellExecute = true,
            //         ErrorDialog = true
            //     };
            //     browser.Start();
            // }
        }
    }
}