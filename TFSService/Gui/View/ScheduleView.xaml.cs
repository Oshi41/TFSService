using Microsoft.TeamFoundation.WorkItemTracking.Client.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    /// Interaction logic for ScheduleView.xaml
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
            {
                if (calendar.SelectedDate == null 
                    || !calendar.DisplayDate.SameMonth(calendar.SelectedDate.Value))
                {
                    calendar.SelectedDate = calendar.DisplayDate;
                }
            }
        }
    }
}
