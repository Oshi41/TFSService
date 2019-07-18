using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Gui.Helper;
using Gui.Properties;
using Gui.Settings;
using Microsoft.Win32;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    /// <summary>
    ///     Настройки приложения. Сохраняет настройки только на кнопку ОК
    /// </summary>
    public class SettingsViewModel : BindableExtended
    {
        private readonly ITfsApi _api;

        private int _capacity;
        private bool _capacityByUser;
        private bool _changed;
        private string _connection;
        private TimeSpan _dayDuration;
        private int _itemMinutesCheck;
        private string _logsPath;
        private int _oldReviewDay;
        private WroteOffStrategy _strategy;

        public SettingsViewModel(string currentConnection, ITfsApi api)
        {
            _api = api;

            ConnectCommand = new ObservableCommand(OnConnect);
            SubmitCommand = new ObservableCommand(OnSave, OnCanSave);
            ChooseLogFileCommand = new ObservableCommand(OnChooseFile);


            Init(currentConnection);
        }

        private void OnChooseFile()
        {
            var dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == true) LogsPath = dlg.FileName;
        }

        /// <summary>
        ///     Обновляем по настройкам
        /// </summary>
        /// <param name="currentConnection"></param>
        private void Init(string currentConnection)
        {
            using (var settings = Settings.Settings.Read())
            {
                _dayDuration = settings.Duration;
                _capacity = settings.Capacity.Hours;
                _connection = currentConnection;
                _strategy = settings.Strategy;
                RuleEditor = new RuleEditorViewModel(settings.Rules);
                Name = _api.Name;
                _logsPath = settings.LogPath;
                _itemMinutesCheck = settings.ItemMinutesCheck;
                _oldReviewDay = settings.OldReviewDay;
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

            if (WindowManager.ShowDialog(vm, Resources.AS_TfsConnection_Title, 400, 200) == true) Connection = vm.Text;
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
            get => _capacity;
            set => SetProperty(ref _capacity, value);
        }

        public bool CapacityByUser
        {
            get => _capacityByUser;
            set => SetProperty(ref _capacityByUser, value);
        }

        public TimeSpan DayDuration
        {
            get => _dayDuration;
            set => SetProperty(ref _dayDuration, value);
        }

        public string Connection
        {
            get => _connection;
            set => SetProperty(ref _connection, value);
        }

        public WroteOffStrategy Strategy
        {
            get => _strategy;
            set => SetProperty(ref _strategy, value);
        }

        public RuleEditorViewModel RuleEditor { get; set; }

        public string Name { get; private set; }

        public string LogsPath
        {
            get => _logsPath;
            set => SetProperty(ref _logsPath, value);
        }

        public int ItemMinutesCheck
        {
            get => _itemMinutesCheck;
            set => SetProperty(ref _itemMinutesCheck, value);
        }

        public int OldReviewDay
        {
            get => _oldReviewDay;
            set => SetProperty(ref _oldReviewDay, value);
        }

        #endregion
    }
}