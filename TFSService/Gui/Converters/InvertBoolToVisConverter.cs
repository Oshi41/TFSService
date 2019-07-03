using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Gui.Converters
{
    internal class InvertBoolToVisConverter : IValueConverter
    {
        private readonly IValueConverter _boolToVis = new BooleanToVisibilityConverter();
        private readonly IValueConverter _invertBoolConverter = new InvertBoolConverter();


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var inverted = _invertBoolConverter.Convert(value, targetType, parameter, culture);
            return _boolToVis.Convert(inverted, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var inverted = _boolToVis.ConvertBack(value, targetType, parameter, culture);
            return _invertBoolConverter.ConvertBack(inverted, targetType, parameter, culture);
        }
    }
}