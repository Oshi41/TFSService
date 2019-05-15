using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Gui.Converters
{
    [ContentProperty(nameof(Cases))]
    class SwitchConverter : FrameworkElement, IValueConverter, IAddChild
    {
        public List<Case> Cases { get; set; } = new List<Case>();

        public static readonly DependencyProperty DefaultProperty = DependencyProperty.Register(
            "Default", typeof(object), typeof(SwitchConverter), new PropertyMetadata(Binding.DoNothing));

        public object Default
        {
            get { return (object) GetValue(DefaultProperty); }
            set { SetValue(DefaultProperty, value); }
        }

        #region IValueConverter
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var statement in Cases)
            {
                if (string.Equals(value?.ToString(), statement.If))
                    return statement.Value;
            }

            return Default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAddChild
        

        public void AddChild(object value)
        {
            if (value is Case c)
            {
                Cases.Add(c);
            }
        }

        public void AddText(string text)
        {
            
        }

        #endregion
    }

    [ContentProperty(nameof(Value))]
    class Case : DependencyObject
    {
        public static readonly DependencyProperty IfProperty = DependencyProperty.Register(
            "If", typeof(string), typeof(Case), new PropertyMetadata(default(string)));

        public string If
        {
            get { return (string) GetValue(IfProperty); }
            set { SetValue(IfProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(Case), new PropertyMetadata(default(object)));

        public object Value
        {
            get { return (object) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
    }
}
