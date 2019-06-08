using System;
using System.Windows;
using System.Windows.Threading;
using Gui.Helper;

namespace Gui.View
{
    /// <summary>
    ///     Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Закрываю окно с отриц. результатом
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Deny(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        ///     Пытаемся закрыть окно на кнопку подтверждения
        /// </summary>
        private void TryClose(object sender, RoutedEventArgs e)
        {
            // Сначала получаем команду
            var command = OkBtn.Command as ObservableCommand;

            // Если команды нет, сразу пытаемся закрыть
            if (command == null)
                Submit();
            else
                command.Executed += OnExecuted;
        }

        private void OnExecuted(object sender, EventArgs e)
        {
            // Сразу отписываемся от события
            if (sender is ObservableCommand command)
                command.Executed -= OnExecuted;

            // После рендера (а значит и отработки все байндингов)
            // пытаемся закрыть окно
            Dispatcher.Invoke(Submit, DispatcherPriority.Loaded);
        }

        private void Submit()
        {
            // Подтверждаем только если кнопка доступна
            if (!OkBtn.IsEnabled)
                return;

            DialogResult = true;
            Close();
        }
    }
}