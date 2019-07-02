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
using Microsoft.TeamFoundation.VersionControl.Common.Internal;
using Microsoft.Win32;
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
        private string logsPath;
        private int itemMinutesCheck;
        private int oldReviewDay;
        private bool _capacityByUser;
        private readonly ITfsApi api;

        public SettingsViewModel(string currentConnection, ITfsApi api)
        {
            this.api = api;

            ConnectCommand = new ObservableCommand(OnConnect);
            SubmitCommand = new ObservableCommand(OnSave, OnCanSave);
            ChooseLogFileCommand = new ObservableCommand(OnChooseFile);


            Init(currentConnection);
        }

        private void OnChooseFile()
        {
            var dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == true)
            {
                LogsPath = dlg.FileName;
            }
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
                capacity = settings.Capacity.Hours;
                connection = currentConnection;
                strategy = settings.Strategy;
                RuleEditor = new RuleEditorViewModel(settings.Rules);
                Name = api.Name;
                logsPath = settings.LogPath;
                itemMinutesCheck = settings.ItemMinutesCheck;
                oldReviewDay = settings.OldReviewDay;
            }
        }

        private void OnSave()
        {
            using (var settings = Settings.Settings.Read())
            {
                settings.Duration = DayDuration;
                settings.Capacity.Hours = Capacity;
                settings.Capacity.ByUser = CapacityByUser;

                if (string.IsNullOrEmpty(Connection)
                    && !settings.Connections.Contains(Connection))
                    settings.Connections.Add(Connection);

                settings.Strategy = Strategy;

                settings.Rules = RuleEditor.Rules;
                settings.LogPath = LogsPath;
                settings.ItemMinutesCheck = ItemMinutesCheck;
                settings.OldReviewDay = OldReviewDay;
            }
        }

        private bool OnCanSave()
        {
            return _changed || RuleEditor.IsChanged;
        }

        private void OnConnect()
        {
            var vm = new FirstConnectionViewModel();

            if (WindowManager.ShowDialog(vm, Properties.Resources.AS_TfsConnection_Title, 400, 200) == true) Connection = vm.Text;
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
        public ICommand ChooseLogFileCommand { get; }

        public int Capacity
        {
            get => capacity;
            set => SetProperty(ref capacity, value);
        }

        public bool CapacityByUser { get => _capacityByUser; set => SetProperty(ref _capacityByUser, value); }

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

        public RuleEditorViewModel RuleEditor { get; set; }

        public string Name { get; private set; }

        public string LogsPath { get => logsPath; set => SetProperty(ref logsPath, value); }

        public int ItemMinutesCheck { get => itemMinutesCheck; set => SetProperty(ref itemMinutesCheck, value); }

        public int OldReviewDay { get => oldReviewDay; set => SetProperty(ref oldReviewDay, value); }

        #endregion
    }
}