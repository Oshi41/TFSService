using System;
using System.Windows;
using Gui.Properties;
using Gui.View;
using Gui.View.Rule_Wizard;
using Gui.ViewModels.DialogViewModels;
using Gui.ViewModels.Notifications;
using Gui.ViewModels.Rules;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace Gui.Helper
{
    /// <summary>
    ///     Статический класс для показа диалогов
    /// </summary>
    public class WindowManager
    {
        /// <summary>
        ///     Показываю диалоговое окно с 2-3 кнопками
        /// </summary>
        /// <param name="vm">Контент</param>
        /// <param name="title">Заголовок окошка</param>
        /// <param name="width">Ширина. Если указать только её, длина будет высчитана автоматически как width * 1.25</param>
        /// <param name="height">Высота. Если указать только её, ширина будет высчитана автоматически как height / 1.25</param>
        /// <param name="ok">Текст кнопки подтверждения</param>
        /// <param name="cancel">Текст кнопки отмены</param>
        /// <param name="specialText">
        ///     Текст вспомогательной кнопки. Если <see cref="BindableExtended.SpecialCommand" /> == <see langword="null" />,
        ///     кнопка не отобразится
        /// </param>
        /// <param name="maximize">Нужно ли расширить экно на весь экран</param>
        /// <returns></returns>
        public static bool? ShowDialog(BindableExtended vm,
            string title,
            double? width = null,
            double? height = null,
            string ok = "OK",
            string cancel = null,
            string specialText = null,
            bool maximize = false)
        {
            var window = new DialogWindow
            {
                Title = title,
                DataContext = vm,
                OkBtn =
                {
                    Content = ok
                },
                ExitBtn =
                {
                    Content = cancel ?? Resources.AS_Cancel
                },
                SpecialBtn =
                {
                    Content = specialText ?? Resources.AS_TryCreate
                }
            };

            if (width == null && height == null)
            {
                width = 400;
                height = 500;
            }
            else
            {
                if (height == null) height = width.Value * 1.25;

                if (width == null) width = height.Value / 1.25;
            }

            window.Width = width.Value;
            window.Height = height.Value;

            CheckAndMaximizeMainWindow();

            if (maximize)
            {
                window.WindowState = WindowState.Maximized;
            }

            return window.ShowDialog();
        }

        /// <summary>
        ///     Показываю диалог создания правила
        /// </summary>
        /// <param name="vm">Контент</param>
        /// <param name="title">Заголовок</param>
        /// <param name="width">Ширина</param>
        /// <param name="height">Высота</param>
        /// <returns></returns>
        public static bool? ShowDialog(AddRuleViewModel vm, string title, double width, double height)
        {
            var window = new RuleWizardView
            {
                DataContext = vm,
                Title = title,
                MinHeight = height,
                MinWidth = width,

                Width = width,
                Height = height
            };

            return window.ShowDialog();
        }

        /// <summary>
        ///     Показываю вспл. уведомление
        /// </summary>
        /// <param name="vm">Контент</param>
        public static void ShowBaloon(BindableNotificationBase vm)
        {
            Notifier.Notify(() => vm);
        }

        /// <summary>
        ///     Показываю текст в случае успеха
        /// </summary>
        /// <param name="text"></param>
        public static void ShowBalloonSuccess(string text)
        {
            Notifier.ShowSuccess(text);
        }

        /// <summary>
        ///     Уведомление в случае неудачи
        /// </summary>
        /// <param name="text"></param>
        public static void ShowBalloonError(string text)
        {
            Notifier.ShowError(text);
        }

        /// <summary>
        ///     Показываю MessageBox с да/нет/отмена решением
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static bool? ShowConfirm(string text, string title)
        {
            switch (MessageBox.Show(text, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    return true;

                case MessageBoxResult.No:
                    return false;

                default:
                    return null;
            }
        }

        #region Private

        /// <summary>
        ///     Класс для показа всплывающих уведомлений
        /// </summary>
        private static readonly Notifier Notifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new PrimaryScreenPositionProvider(Corner.BottomRight, 10, 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromMinutes(1),
                MaximumNotificationCount.FromCount(10));
        });

        private static void CheckAndMaximizeMainWindow()
        {
            var main = Application.Current?.MainWindow;

            if (main == null)
                return;

            if (main.WindowState == WindowState.Minimized) main.WindowState = WindowState.Normal;
        }

        #endregion
    }
}