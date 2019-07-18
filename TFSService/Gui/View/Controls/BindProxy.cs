using System.Windows;

namespace Gui.View.Controls
{
    internal class BindProxy : Freezable
    {
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            "Data", typeof(object), typeof(BindProxy), new PropertyMetadata(default(object)));

        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BindProxy();
        }
    }
}