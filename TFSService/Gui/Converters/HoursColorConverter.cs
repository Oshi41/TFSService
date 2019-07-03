using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Gui.ViewModels.DialogViewModels;

namespace Gui.Converters
{
    /// <summary>
    ///     Конвертер для цвета числа в календаре.
    /// </summary>
    internal class HoursColorConverter : IValueConverter
    {
        private Brush _lessColor;
        private Brush _normalColor;
        private Brush _overflowColor;
        private Brush _zeroColor;

        /// <summary>
        ///     Переполнили трудозатраты
        /// </summary>
        public Brush OverflowColor
        {
            get => _overflowColor;
            set
            {
                _overflowColor = value.Clone();
                _overflowColor.Opacity = 0.5;
            }
        }

        /// <summary>
        ///     Уложились в трудозатраты
        /// </summary>
        public Brush NormalColor
        {
            get => _normalColor;
            set
            {
                _normalColor = value.Clone();
                _normalColor.Opacity = 0.5;
            }
        }

        /// <summary>
        ///     Списали меньше, чем надо
        /// </summary>
        public Brush LessColor
        {
            get => _lessColor;
            set
            {
                _lessColor = value.Clone();
                _lessColor.Opacity = 0.5;
            }
        }

        /// <summary>
        ///     Вообще ничего не списали
        /// </summary>
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
            if (!(value is DayViewModel vm)) return Binding.DoNothing;

            if (vm.IsHolliday || vm.Capacity == vm.Hours)
                return NormalColor;

            if (vm.Capacity > 0 && vm.Hours < 1) return ZeroColor;

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