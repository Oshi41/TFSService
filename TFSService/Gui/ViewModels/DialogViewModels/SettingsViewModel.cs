using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Gui.Helper;
using Gui.Settings;
using Gui.ViewModels.Rules;
using TfsAPI.Interfaces;
using TfsAPI.RulesNew;

namespace Gui.ViewModels.DialogViewModels
{
    public class SettingsViewModel : BindableExtended
    {
        private bool _changed;

        private int capacity;
        private string connection;
        private TimeSpan dayDuration;
        private WroteOffStrategy strategy;
        private List<IRule> rules;
        private readonly ITfsApi api;

        public SettingsViewModel(string currentConnection, ITfsApi api)
        {
            Init(currentConnection);

            ConnectCommand = new ObservableCommand(OnConnect);
            SubmitCommand = new ObservableCommand(OnSave, () => _changed);
            AddRuleCommand = new ObservableCommand(OnAdd);
            this.api = api;
        }

        

        /// <summary>
        ///     Обновляем по настройкам
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
                rules = settings.Rules.ToList();
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
                    settings.Connections.Add(Connection);

                settings.Strategy = Strategy;
            }
        }

        private void OnConnect()
        {
            var vm = new FirstConnectionViewModel();

            if (WindowManager.ShowDialog(vm, "TFS соединение", 400, 200) == true) Connection = vm.Text;
        }

        private void OnAdd()
        {
            var vm = new AddRuleViewModel();

            if (WindowManager.ShowDialog(vm, "Мастер добавления правила", 400, 300) == true)
            {
                var builder = new RuleBuilder(api);

                if (vm.UsePresets)
                {
                    var rule = builder.BuildPresets(vm.Preset);

                    using (var settings = Settings.Settings.Read())
                    {
                        if (settings.Rules.Contains(rule))
                        {
                            // todo ask user
                            if (WindowManager.ShowConfirm("Заменить правило?", "Замена") == true)
                            {
                                settings.Rules.Remove(rule);
                            }
                            else
                            {
                                return;
                            }
                        }

                        settings.Rules.Add(rule);
                    }
                }
            }
        }

        protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            var result = base.SetProperty(ref storage, value, propertyName);

            if (result && !_changed) _changed = true;

            return result;
        }

        #region Properties

        /// <summary>
        ///     Подключение к другому TFS
        /// </summary>
        public ICommand ConnectCommand { get; }
        public ICommand AddRuleCommand { get; }
        

        public int Capacity
        {
            get => capacity;
            set => SetProperty(ref capacity, value);
        }

        public TimeSpan DayDuration
        {
            get => dayDuration;
            set => SetProperty(ref dayDuration, value);
        }

        public string Connection
        {
            get => connection;
            set => SetProperty(ref connection, value);
        }

        public WroteOffStrategy Strategy
        {
            get => strategy;
            set => SetProperty(ref strategy, value);
        }

        public List<IRule> Rules { get => rules; set => SetProperty(ref rules, value); }

        #endregion
    }
}