using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Gui.View.Notifications;
using MaterialDesignThemes.Wpf;
using Mvvm;
using ToastNotifications.Core;

namespace Gui.ViewModels.Notifications
{
    public class BindableNotificationBase : NotificationBase, INotifyPropertyChanged, INotification
    {
        private NotificationDisplayPart _displayPart;
        private PackIconKind? _icon;

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
        
        protected BindableNotificationBase(string message) 
            : base(message, new MessageOptions())
        {
            Options.FontSize = 11;
            Options.ShowCloseButton = true;
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
    }
}
