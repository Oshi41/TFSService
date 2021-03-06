﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Gui.View.Notifications;
using ToastNotifications.Core;

namespace Gui.ViewModels.Notifications
{
    /// <summary>
    ///     Базовый класс для уведомлений
    /// </summary>
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

        public new EnhancedOptions Options => (EnhancedOptions) base.Options;

        /// <summary>
        ///     Визуальное представление уведомления.
        /// </summary>
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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Изменяю свойство и вызываю метод обновления, если был передан
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">Источник</param>
        /// <param name="value">Новое значение</param>
        /// <param name="onChange">Метод, вызывающийся при изменениии</param>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns></returns>
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

    /// <summary>
    ///     Расширенные возможности для уведомлений
    /// </summary>
    public class EnhancedOptions : MessageOptions
    {
        public Action<BindableNotificationBase> DoubleClickAction { get; set; }
    }
}