using Gui.ViewModels.DialogViewModels;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using TfsAPI.Extentions;

namespace Gui.Behaviors
{
    class CalendarItemSourceBehavior : Behavior<Calendar>
    {
        public IList Items
        {
            get { return (IList)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IList), typeof(CalendarItemSourceBehavior),
                new FrameworkPropertyMetadata(OnItemsChanged));

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CalendarItemSourceBehavior behavior))
                return;

            if (e.OldValue is INotifyCollectionChanged old)
            {
                old.CollectionChanged -= behavior.ThrowBinding;
            }

            if (e.NewValue is INotifyCollectionChanged add)
            {
                add.CollectionChanged += behavior.ThrowBinding;                
            }

            behavior.ThrowBinding(behavior, EventArgs.Empty);
        }

        public static object GetDaylyDataContext(DependencyObject obj)
        {
            return (object)obj.GetValue(DaylyDataContextProperty);
        }

        public static void SetDaylyDataContext(DependencyObject obj, object value)
        {
            obj.SetValue(DaylyDataContextProperty, value);
        }

        public static readonly DependencyProperty DaylyDataContextProperty =
            DependencyProperty.RegisterAttached("DaylyDataContext", typeof(object), typeof(CalendarItemSourceBehavior), new PropertyMetadata(null));

        #region overrided

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.DisplayDateChanged += ThrowBinding;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.DisplayDateChanged -= ThrowBinding;

            base.OnDetaching();
        }

        #endregion

        private Grid MonthView
        {
            get
            {
                var name = "PART_MonthView";
                var find = Gui.Helper.WpfUtil.FindChildByName<Grid>(AssociatedObject, name);
                return find;
            }
        }

        private void ThrowBinding(object sender, EventArgs e)
        {
            var month = MonthView;

            foreach (var item in month.Children.OfType<CalendarDayButton>())
            {
                // Очищаем
                SetDaylyDataContext(item, null);

                if (item.DataContext is DateTime itemTime)
                {
                    var first = Items?.OfType<ITimable>().FirstOrDefault(x => x.Time.IsToday(itemTime));
                    if (first != null)
                    {
                        SetDaylyDataContext(item, first);
                    }
                }
            }
        }
    }
}
