using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Gui.Converters
{
    /// <summary>
    ///     Получаем ID (int) из рабочего элемента
    /// </summary>
    internal class IdWorkitemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WorkItem item) return item.Id;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}