using System.ComponentModel;
using Gui.View;
using Gui.ViewModels;
using Microsoft.TeamFoundation.Client;
using Mvvm;

namespace Gui.Helper
{
    public class WindowManager
    {
        public static bool? ShowDialog(BindableExtended vm, 
            string title,
            double? width = null,
            double? height = null,
            string ok = "OK",
            string cancel = "Отмена")
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
                }
            };

            if (width == null && height == null)
            {
                width = 400;
                height = 500;
            }
            else
            {
                if (height == null)
                {
                    height = width.Value * 1.25;
                }

                if (width == null)
                {
                    width = height.Value / 1.25;
                }
            }

            window.Width = width.Value;
            window.Height = height.Value;

            return window.ShowDialog();
        }
    }
}
