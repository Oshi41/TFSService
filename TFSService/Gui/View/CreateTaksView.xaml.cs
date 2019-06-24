using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gui.View
{
    /// <summary>
    ///     Interaction logic for CreateTaksView.xaml
    /// </summary>
    public partial class CreateTaksView : UserControl
    {
        public CreateTaksView()
        {
            InitializeComponent();
        }

        private void AllowOnlyNumbers(object sender, TextCompositionEventArgs e)
        {
            e.Handled = e.Text.Any(c => !char.IsNumber(c));
        }
    }
}