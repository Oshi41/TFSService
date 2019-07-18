using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Gui.Converters
{
    /// <summary>
    ///     Конвертер с коллекцией ключ-значние. Проверяет строковое представление. Должен сработать и с null (не
    ///     протестировано)
    /// </summary>
    [ContentProperty(nameof(Cases))]
    internal class SwitchConverter : FrameworkElement, IValueConverter, IAddChild
    {
        public static readonly DependencyProperty DefaultProperty = DependencyProperty.Register(
            "Default", typeof(object), typeof(SwitchConverter), new PropertyMetadata(Binding.DoNothing));

        public List<Case> Cases { get; set; } = new List<Case>();

        public object Default
        {
            get => GetValue(DefaultProperty);
            set => SetValue(DefaultProperty, value);
        }

        #region IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Идет проверка по Equals объекта
            var first = Cases.FirstOrDefault(x => Equals(x?.If, value));
            if (first == null)
            {
                // иначе проверяем как строку
                var strVal = value?.ToString();
                first = Cases.FirstOrDefault(x => string.Equals(strVal, x?.If?.ToString()));
            }

            return first == null
                ? Default
                : first.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAddChild

        public void AddChild(object value)
        {
            if (value is Case c) Cases.Add(c);
        }

        public void AddText(string text)
        {
        }

        #endregion
    }

    [ContentProperty(nameof(Value))]
    internal class Case : DependencyObject
    {
        public static readonly DependencyProperty IfProperty = DependencyProperty.Register(
            "If", typeof(object), typeof(Case), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(Case), new PropertyMetadata(default(object)));

        public object If
        {
            get => GetValue(IfProperty);
            set => SetValue(IfProperty, value);
        }

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
    }
}