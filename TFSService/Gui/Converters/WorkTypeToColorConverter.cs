using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TfsAPI.Extentions;

namespace Gui.Converters
{
    class WorkTypeToColorConverter : IValueConverter
    {
        /// <summary>
        /// обычный фон
        /// </summary>
        public Brush RegularBrush { get; set; } = Brushes.Transparent;

        /// <summary>
        /// Если есть баг
        /// </summary>
        public Brush BugBrush { get; set; } = Brushes.LightPink;        

        /// <summary>
        /// Если есть PBi и похожие элементы
        /// </summary>
        public Brush WorkBrush { get; set; } = Brushes.LightGreen;

        /// <summary>
        /// Критически важные рабочие элементы
        /// </summary>
        public Brush CryticalBrush { get; set; } = Brushes.Orange;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList<WorkItem> items)
            {
                // ОТ высшего приоритета и ниже

                if (items.Any(x => x.IsRare()))
                {
                    return CryticalBrush;
                }

                if (items.Any(x => x.IsWorkItem()))
                {
                    return WorkBrush;
                }

                if (items.Any(x => x.IsBug()))
                {
                    return BugBrush;
                }

                return RegularBrush;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
