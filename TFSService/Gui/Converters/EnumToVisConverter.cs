using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Gui.Converters
{
    /// <summary>
    ///     Видимость в зависимости от enum и передаваемого параметра. Можно настроить желаемые значения, в том чиле и
    ///     инвертировать
    /// </summary>
    public class EnumToVisConverter : IValueConverter
    {
        public Visibility EqualsVisibility { get; set; } = Visibility.Visible;
        public Visibility NotEqualsVisibility { get; set; } = Visibility.Collapsed;


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.Equals(value?.ToString(), parameter?.ToString()))
                return EqualsVisibility;

            return NotEqualsVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}