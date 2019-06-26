using System;
using System.Windows;
using Gui.View;
using Gui.View.Rule_Wizard;
using Gui.ViewModels.DialogViewModels;
using Gui.ViewModels.Notifications;
using Gui.ViewModels.Rules;
using Microsoft.TeamFoundation.Build.WebApi;
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

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromMinutes(1),
                MaximumNotificationCount.FromCount(10));
        });

        public static bool? ShowDialog(BindableExtended vm,
            string title,
            double? width = null,
            double? height = null,
            string ok = "OK",
            string cancel = null,
            string specialText = null)
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
                    Content = cancel ?? Properties.Resources.AS_Cancel
                },
                SpecialBtn =
                {
                    Content = specialText ?? Properties.Resources.AS_TryCreate
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

        public static bool? ShowDialog(AddRuleViewModel vm, string title, double width, double height)
        {
            var window = new RuleWizardView
            {
                DataContext = vm,
                Title = title,
                MinHeight = height,
                MinWidth = width,

                Width = width,
                Height = height,
            };

            return window.ShowDialog();
        }

        public static void ShowBaloon(BindableNotificationBase vm)
        {
            _notifier.Notify(() => vm);
        }

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
    }
}