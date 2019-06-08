using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Gui.Converters
{
    public class DescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null) return GetDescr(value);

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetDescr(object value)
        {
            try
            {
                var type = value.GetType();
                var field = type.GetField(value.ToString());
                var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                var first = attributes.Cast<DescriptionAttribute>().First();

                return first.Description;
            }
            catch
            {
                return value?.ToString() ?? string.Empty;
            }
        }
    }
}