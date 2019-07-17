using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Gui.Converters
{
    /// <summary>
    /// Класс хранения ключ - значение для XAML
    /// </summary>
    /// <typeparam name="TKey">Тип ключа</typeparam>
    /// <typeparam name="TValue">Тип значения</typeparam>
    public class KeyValue<TKey, TValue> : DependencyObject
    {
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(TKey), typeof(KeyValue<TKey, TValue>),
                new PropertyMetadata(default(TKey)));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(TValue), typeof(KeyValue<TKey, TValue>),
                new PropertyMetadata(default(TValue)));

        /// <summary>
        /// Ключ
        /// </summary>
        public TKey Key
        {
            get { return (TKey)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        /// <summary>
        /// Значение
        /// </summary>
        public TValue Value
        {
            get { return (TValue)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
    }

    /// <summary>
    /// Объектовые ключ - объектовые значение (для XAML)
    /// </summary>
    public class KeyValueObject : KeyValue<object, object>
    {
    }

    /// <summary>
    /// Входящее значение используется в качестве ключа и ищет соответствующее значение
    /// </summary>
    [ContentProperty("Items")]
    public class KeyToValueConverter : DependencyObject, IValueConverter, IAddChild
    {
        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register("DefaultValue", typeof(object), typeof(KeyToValueConverter),
                new PropertyMetadata(default(object)));

        public KeyToValueConverter()
        {
            Items = new List<KeyValueObject>();
        }

        /// <summary>
        /// Значение по умолчанию или когда нет соответсвующего ключа
        /// </summary>
        public object DefaultValue
        {
            get { return GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }


        /// <summary>
        /// Список ключей и значений
        /// </summary>
        public IList Items { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueObjects = Items.OfType<KeyValueObject>().ToList();
            int findIndex;
            try
            {
                findIndex = valueObjects.FindIndex(v => v.Key.Equals(value));
            }
            catch (Exception)
            {
                findIndex = valueObjects.FindIndex(x => Equals(x.Key, value));
            }
            return findIndex == -1 ? DefaultValue : valueObjects[findIndex].Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueObjects = Items.OfType<KeyValueObject>().ToList();
            var keyValue = valueObjects.FirstOrDefault(v => Equals(v.Value, value));

            return keyValue == null
                ? DependencyProperty.UnsetValue
                : keyValue.Key;
        }

        public void AddChild(object value)
        {
            var keyValue = value as KeyValueObject;
            if (keyValue == null)
                throw new NotSupportedException(string.Format("Type: {0}", value.GetType()));
            Items.Add(keyValue);
        }

        public void AddText(string text)
        {
            throw new NotImplementedException();
        }
    }
}
