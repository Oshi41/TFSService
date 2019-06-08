using System.Windows.Controls;
using TfsAPI.Extentions;

namespace Gui.View
{
    /// <summary>
    ///     Interaction logic for ScheduleView.xaml
    /// </summary>
    public partial class ScheduleView : UserControl
    {
        public ScheduleView()
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
    }
}