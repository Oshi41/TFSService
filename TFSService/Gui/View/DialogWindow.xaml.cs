using System;
using System.Windows;
using System.Windows.Threading;
using Gui.Helper;

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

        private void TryClose(object sender, RoutedEventArgs e)
        {
            var command = OkBtn.Command as ObservableCommand;
            var enabled = OkBtn.IsEnabled;

            if (!enabled)
                return;

            if (command == null)
            {
                Submit();
            }
            else
            {
                command.Executed += OnExecuted;
            }
        }

        private void OnExecuted(object sender, EventArgs e)
        {
            if (sender is ObservableCommand command)
                command.Executed -= OnExecuted;

            Dispatcher.Invoke(() =>
            {
                if (OkBtn.IsEnabled)
                    Submit();

            }, DispatcherPriority.Loaded);
            
        }

        private void Submit()
        {
            this.DialogResult = true;
            Close();
        }
    }
}
