using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TfsAPI.Extentions;

namespace Gui.View
{
    /// <summary>
    /// Interaction logic for TrendView.xaml
    /// </summary>
    public partial class TrendView : UserControl
    {
        public TrendView()
        {
            InitializeComponent();
        }

        private void SelectFirst(object sender, CalendarDateChangedEventArgs e)
        {
            if (sender is Calendar calendar)
                if (calendar.SelectedDate == null
                    || !calendar.DisplayDate.SameMonth(calendar.SelectedDate.Value))
                    calendar.SelectedDate = calendar.DisplayDate;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            // Прячу кнопку переключения видов
            if (sender is Calendar calendar
                && calendar.Template.FindName("PART_CalendarItem", calendar) is CalendarItem item
                && item.Template.FindName("PART_HeaderButton", item) is FrameworkElement button)
            {
                button.Visibility = Visibility.Collapsed;
            }
        }
    }
}