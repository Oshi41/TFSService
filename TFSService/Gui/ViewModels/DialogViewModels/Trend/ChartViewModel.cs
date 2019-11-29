using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using Mvvm;
using TfsAPI.TFS.Trend;

namespace Gui.ViewModels
{
    public class ChartViewModel : BindableBase
    {
        private readonly DateTime _start;
        private bool _disableAnimation;
        public Func<double, string> BottomLabelFormatter { get; }
        public Func<double, string> UpLabelFormatter { get; }

        public SeriesCollection Series { get; }

        public bool DisableAnimation
        {
            get => _disableAnimation;
            set => SetProperty(ref _disableAnimation, value);
        }

        public ChartViewModel(Chart model)
        {
            _start = model.Available.FirstOrDefault()?.Time ?? DateTime.Now;

            BottomLabelFormatter = day => _start.AddDays(day).ToShortDateString();
            UpLabelFormatter = hours => $"{hours} ч.";

            Series = new SeriesCollection
            {
                CreateByList(model.Available.Select(x => x.Value), Properties.Resources.AS_AvailableCapacity, Brushes.Green, Brushes.Transparent),
                CreateByList(model.Items.Select(x => x.Value), Properties.Resources.AS_RemainingWork, Brushes.DodgerBlue),
                CreateByList(model.WriteOff.Select(x => x.Value), Properties.Resources.AS_WriteOff_Trend, Brushes.DarkRed),
            };

            DisableAnimation = model.Available.Count > 31;
        }

        private LineSeries CreateByList<T>(IEnumerable<T> valuesByDay, string title, Brush stroke, Brush fill = null)
        {
            var result = new LineSeries
            {
                Title = title,
                Values = new ChartValues<T>(valuesByDay),
                Stroke = stroke,
            };

            if (fill != null)
            {
                result.Fill = fill;
            }

            return result;
        }
    }
}
