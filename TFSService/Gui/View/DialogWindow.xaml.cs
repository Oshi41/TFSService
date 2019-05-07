using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Gui.ViewModels;
using MaterialDesignThemes.Wpf;

namespace Gui.View
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
        }


        private void Deny(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void OnCanClose(object sender, ExecutedRoutedEventArgs e)
        {
            if (OkBtn.IsEnabled)
            {
                this.DialogResult = true;
                Close();
            }
        }
    }
}
