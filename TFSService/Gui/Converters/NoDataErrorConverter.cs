using System;
using System.Globalization;
using System.Windows.Data;

namespace Gui.Converters
{
    /// <summary>
    ///     Конвертер для индикации наличия ошибки Data Error.
    ///     Проверяет на нуль или на пустую сроку
    /// </summary>
    internal class NoDataErrorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return true;

            if (value is string s)
                return string.IsNullOrEmpty(s);

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}