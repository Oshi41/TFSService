using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Mvvm;
using ToastNotifications.Core;

namespace Gui.ViewModels.Notifications
{
    public class BindableNotificationBase : NotificationBase, INotifyPropertyChanged, INotification
    {
        private NotificationDisplayPart _displayPart;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(ref T source, T value, Action<T, T> onChange,
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
            Options.FontSize = 20;
            Options.ShowCloseButton = true;
        }

        public override NotificationDisplayPart DisplayPart
        {
            get
            {
                if (_displayPart == null)
                {
                    var key = new DataTemplateKey(GetType());

                    if (App.Current.TryFindResource(key) is DataTemplate template 
                        && template.LoadContent() is NotificationDisplayPart loaded)
                    {
                        _displayPart = loaded;
                        _displayPart.Bind(this);
                    }
                }

                return _displayPart;
            }
        }
    }
}
