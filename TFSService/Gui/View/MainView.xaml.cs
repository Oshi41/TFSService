using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Gui.ViewModels;

namespace Gui.View
{
    /// <summary>
    ///     Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private NotifyIcon _trayIcon;

        public MainView()
        {
            InitializeComponent();

            _trayIcon = new NotifyIcon
            {
                Icon = Properties.Resources.logo,
                ContextMenu = new ContextMenu(new[]
                {
                    new MenuItem(Properties.Resources.AS_Open, OnOpenWindow),
                    new MenuItem("-"),
                    new MenuItem(Properties.Resources.AS_Update, OnUpdate),
                    new MenuItem("-"),
                    new MenuItem(Properties.Resources.AS_ShutdownProgram, (sender, args) => Close()),
                })
            };

            var titleAttr = GetType()
                .GetTypeInfo()
                .Assembly
                .GetCustomAttributes(typeof(AssemblyTitleAttribute))
                .OfType<AssemblyTitleAttribute>()
                .FirstOrDefault();

            _trayIcon.Text = titleAttr?.Title;
        }

        #region Overrides of Window

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            // Показываем иконку и прячем приложение, как скайп
            _trayIcon.Visible = WindowState == WindowState.Minimized;
            ShowInTaskbar = !_trayIcon.Visible;
        }

        #endregion

        #region Menu methods

        private void OnOpenWindow(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.UpdateCommand.Execute(sender);
            }
        }

        #endregion
    }
}