using Gui.Helper;
using Gui.Settings;
using Microsoft.TeamFoundation.VersionControl.Common.Internal;
using Mvvm.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gui.ViewModels.DialogViewModels
{
    public class SettingsViewModel : BindableExtended
    {
        private bool _changed;

        private int capacity;
        private TimeSpan dayDuration;
        private string connection;
        private WroteOffStrategy strategy;

        #region Properties

        /// <summary>
        /// Подключение к другому TFS
        /// </summary>
        public ICommand ConnectCommand { get; }

        public int Capacity { get => capacity; set => SetProperty(ref capacity, value); }

        public TimeSpan DayDuration { get => dayDuration; set => SetProperty(ref dayDuration, value); }

        public string Connection { get => connection; set => SetProperty(ref connection, value); }

        public WroteOffStrategy Strategy { get => strategy; set => SetProperty(ref strategy, value); }

        #endregion

        public SettingsViewModel(string currentConnection)
        {
            Init(currentConnection);

            ConnectCommand = new ObservableCommand(OnConnect);
            SubmitCommand = new ObservableCommand(OnSave, () => _changed);
        }

        /// <summary>
        /// Обновляем по настройкам
        /// </summary>
        /// <param name="currentConnection"></param>
        private void Init(string currentConnection)
        {
            using (var settings = Settings.Settings.Read())
            {
                dayDuration = settings.Duration;
                capacity = settings.Capacity;
                connection = currentConnection;
                strategy = settings.Strategy;
            }
        }

        private void OnSave()
        {
            using (var settings = Settings.Settings.Read())
            {
                settings.Duration = DayDuration;
                settings.Capacity = Capacity;

                if (string.IsNullOrEmpty(Connection)
                    && !settings.Connections.Contains(Connection))
                {
                    settings.Connections.Add(Connection);
                }

                settings.Strategy = Strategy;
            }
        }

        private void OnConnect()
        {
            var vm = new FirstConnectionViewModel();

            if (WindowManager.ShowDialog(vm, "TFS соединение", 400, 200) == true)
            {
                Connection = vm.Text;
            }
        }

        protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            var result = base.SetProperty(ref storage, value, propertyName);

            if (result && !_changed)
            {
                _changed = true;
            }

            return result;
        }
    }
}
