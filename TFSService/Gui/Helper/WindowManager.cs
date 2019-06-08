using System;
using Gui.View;
using Gui.ViewModels.DialogViewModels;
using Gui.ViewModels.Notifications;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace Gui.Helper
{
    public class WindowManager
    {
        private static readonly Notifier _notifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new PrimaryScreenPositionProvider(Corner.BottomRight, 10, 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromSeconds(5),
                MaximumNotificationCount.FromCount(10));
        });

        public static bool? ShowDialog(BindableExtended vm,
            string title,
            double? width = null,
            double? height = null,
            string ok = "OK",
            string cancel = "Отмена",
            string specialText = "Создать...")
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
                    Content = cancel
                },
                SpecialBtn =
                {
                    Content = specialText
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

            return window.ShowDialog();
        }

        public static void ShowBaloon(BindableNotificationBase vm)
        {
            _notifier.Notify(() => vm);
        }
    }
}