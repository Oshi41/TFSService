using System;
using System.Globalization;
using System.Windows.Data;
using Gui.ViewModels.DialogViewModels;
using Brush = System.Windows.Media.Brush;

namespace Gui.Converters
{
    class HoursColorConverter : IValueConverter
    {
        private Brush _overflowColor;
        private Brush _normalColor;
        private Brush _lessColor;
        private Brush _zeroColor;

        public Brush OverflowColor
        {
            get => _overflowColor;
            set
            {
                _overflowColor = value.Clone();
                _overflowColor.Opacity = 0.5;
            }
        }

        public Brush NormalColor
        {
            get => _normalColor;
            set
            {
                _normalColor = value.Clone();
                _normalColor.Opacity = 0.5;
            }
        }

        public Brush LessColor
        {
            get => _lessColor;
            set
            {
                _lessColor = value.Clone();
                _lessColor.Opacity = 0.5;
            }
        }

        public Brush ZeroColor
        {
            get => _zeroColor;
            set
            {
                _zeroColor = value.Clone();
                _zeroColor.Opacity = 0.5;
            }
        }

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
