using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Gui.Settings;
using Microsoft.TeamFoundation.Client.CommandLine;
using Mvvm.Commands;

namespace Gui.View.Controls
{
    public partial class CapacitySettingsView : UserControl, IWeakEventListener
    {
        private static readonly IList<WeakReference<IWeakEventListener>> _listeners = new List<WeakReference<IWeakEventListener>>();

        public static readonly DependencyProperty CapacityProperty = DependencyProperty.Register("Capacity",
            typeof(int),
            typeof(CapacitySettingsView),
            new FrameworkPropertyMetadata(default(int),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is int old && e.NewValue is int val)
            {
                if (old != val)
                {
                    using (var settings = new WriteOffSettings().Read<WriteOffSettings>())
                    {
                        settings.Capacity = TimeSpan.FromHours(val);

                        if (d is CapacitySettingsView view)
                        {
                            view.OnChange(val);
                        }
                    }
                }
            }
        }

        private void OnChange(object sender)
        {
            if (sender == null)
                return;

            for (var i = 0; i < _listeners.Count; i++)
            {
                if (!_listeners[i].TryGetTarget(out var listener))
                {
                    _listeners.RemoveAt(i);
                    i--;
                }
                else
                {
                    if (!Equals(sender, this))
                    {
                        listener.ReceiveWeakEvent(null, sender, null);
                    }
                }
            }
        }

        public CapacitySettingsView()
        {
            InitializeComponent();

            _listeners.Add(new WeakReference<IWeakEventListener>(this));
        }

        public int Capacity
        {
            get => (int)GetValue(CapacityProperty);
            set => SetValue(CapacityProperty, value);
        }

        private void AfterLoaded(object sender, RoutedEventArgs e)
        {
            Capacity = (int)new WriteOffSettings().Read<WriteOffSettings>().Capacity.TotalHours;
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (sender is int value && Capacity != value)
            {
                Capacity = value;
                return true;
            }

            return false;
        }
    }
}