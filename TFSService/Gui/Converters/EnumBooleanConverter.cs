using System;
using System.Windows.Data;

namespace Gui.Converters
{
    /// <summary>
    /// Конвертирует  перечесление в bool?-значение по параметру.
    /// </summary>
    public class EnumBooleanConverter : IValueConverter
    {
        public object TrueValue { get; set; }
        public object FalseValue { get; set; }
        public object NullValue { get; set; }

        public EnumBooleanConverter()
        {
            TrueValue = null;
            FalseValue = null;
            NullValue = null;
        }
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (object.Equals(value, NullValue))
                return null;

            if (TrueValue != null && parameter == null)
                return object.Equals(value, TrueValue);

            return object.Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return NullValue;

            return value.Equals(true) ? (parameter ?? TrueValue) : (FalseValue ?? Binding.DoNothing);
        }

        #endregion
    }
}
