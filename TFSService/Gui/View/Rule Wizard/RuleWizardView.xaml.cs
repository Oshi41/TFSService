using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gui.View.Rule_Wizard
{
    /// <summary>
    /// Interaction logic for RuleWizardView.xaml
    /// </summary>
    public partial class RuleWizardView
    {
        public RuleWizardView()
        {
            InitializeComponent();
        }

        private void TryClose(object sender, ExecutedRoutedEventArgs e)
        {
            if (DoneBtn.IsEnabled)
            {
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
