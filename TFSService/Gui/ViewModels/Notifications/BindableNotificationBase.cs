using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Gui.View.Notifications;
using ToastNotifications.Core;

namespace Gui.ViewModels.Notifications
{
    public class BindableNotificationBase : NotificationBase, INotifyPropertyChanged, INotification
    {
        private NotificationDisplayPart _displayPart;

        protected BindableNotificationBase(string message)
            : base(message, new EnhancedOptions())
        {
            Options.FontSize = 11;
            Options.ShowCloseButton = true;
            Options.FreezeOnMouseEnter = true;
        }

        public override NotificationDisplayPart DisplayPart
        {
            get
            {
                if (_displayPart == null)
                {
                    _displayPart = new ToastViewBase();
                    _displayPart.Bind(this);

                    _displayPart.DataContext = this;
                }

                return _displayPart;
            }
        }

        public new EnhancedOptions Options => (EnhancedOptions)base.Options;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(ref T source, T value, Action<T, T> onChange = null,
            [CallerMemberName] string propertyName = null)
        {
            if (Equals(source, value))
                return false;

            onChange?.Invoke(source, value);
            source = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }

    public class EnhancedOptions : MessageOptions
    {
        public Action<BindableNotificationBase> DoubleClickAction { get; set; }
    }
}