using System;
using System.Globalization;
using System.Windows.Data;

namespace Gui.Converters
{
    /// <summary>
    ///     Конвертирует  перечесление в bool?-значение по параметру.
    /// </summary>
    public class EnumBooleanConverter : IValueConverter
    {
        public EnumBooleanConverter()
        {
            TrueValue = null;
            FalseValue = null;
            NullValue = null;
        }

        public object TrueValue { get; set; }
        public object FalseValue { get; set; }
        public object NullValue { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Equals(value, NullValue))
                return null;

            if (TrueValue != null && parameter == null)
                return Equals(value, TrueValue);

            return Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return NullValue;

            return value.Equals(true)
                ? parameter ?? TrueValue
                : FalseValue ?? Binding.DoNothing;
        }

        #endregion
    }
}