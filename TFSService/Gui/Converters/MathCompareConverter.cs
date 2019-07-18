using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Gui.Converters
{
    [Flags]
    public enum OperationTypes
    {
        More,
        Less,
        Equals
    }

    /// <summary>
    ///     Булевские сравнения между двумя переменными.
    ///     Пока что флаги не поддерживает
    /// </summary>
    public class MathCompareConverter : IMultiValueConverter
    {
        public OperationTypes Operation { get; set; }

        public object TrueValue { get; set; } = true;
        public object FalseValue { get; set; } = false;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null
                && values.Count() >= 2
                && double.TryParse(values[0]?.ToString(), out var x)
                && double.TryParse(values[1]?.ToString(), out var y))
                return GetResult(x, y, Operation)
                    ? TrueValue
                    : FalseValue;

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private bool GetResult(double x, double y, OperationTypes singleOperation)
        {
            switch (Operation)
            {
                case OperationTypes.More:
                    return x > y;
                case OperationTypes.Less:
                    return x < y;
                case OperationTypes.Equals:
                    return x == y;

                default:
                    throw new Exception("Unknonw situation");
            }
        }
    }
}