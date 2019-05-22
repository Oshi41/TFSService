using System;
using System.Globalization;
using System.Windows.Data;
using Gui.ViewModels.DialogViewModels;
using Brush = System.Windows.Media.Brush;

namespace Gui.Converters
{
    class HoursColorConverter : IValueConverter
    {
        public Brush OverflowColor { get; set; }
        public Brush NormalColor { get; set; }
        public Brush LessColor { get; set; }
        public Brush ZeroColor { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DayViewModel vm))
            {
                return Binding.DoNothing;
            }

            if (vm.IsHolliday || vm.Capacity == vm.Hours)
                return NormalColor;

            if (vm.Capacity > 0 && vm.Hours < 1)
            {
                return ZeroColor;
            }

            return vm.Capacity > vm.Hours
                ? LessColor
                : OverflowColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
