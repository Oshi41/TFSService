using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Runtime.CompilerServices;
using Gui.Helper;
using Mvvm;
using Newtonsoft.Json;
using TfsAPI.Extentions;
using TfsAPI.RulesNew;

namespace Gui.Settings
{
    public class Settings : BindableBase, IDisposable
    {
        public Settings()
        {
            CompletedWork = new WriteOffCollection();
            Connections = new ObservableCollection<string>();
            MyWorkItems = new ObservableCollection<int>();
            Rules = new ObservableCollection<IRule>();
        }

        #region Fields

        private static readonly string _savePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TfsService",
            "config.json");

        private int _capacity;
        private TimeSpan _duration;
        private DateTime _begin;

        private bool _changed;
        private ObservableCollection<string> _connections;
        private WriteOffCollection _completedWork;
        private ObservableCollection<int> _myWorkItems;
        private WroteOffStrategy strategy;
        private string logPath = Path.Combine(Path.GetDirectoryName(_savePath), "logs.log");
        private ObservableCollection<IRule> rules;

        #endregion

        #region Properties

        /// <summary>
        ///     Начало рабочего дня
        /// </summary>
        public DateTime Begin
        {
            get => _begin;
            set => SetProperty(ref _begin, value);
        }

        /// <summary>
        ///     Продолжительность рабочего дня
        /// </summary>
        public TimeSpan Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        /// <summary>
        ///     Сколько часов надо списать
        /// </summary>
        public int Capacity
        {
            get => _capacity;
            set => SetProperty(ref _capacity, value);
        }

        /// <summary>
        ///     Сколько часов было списано на разные рабочие элементы
        /// </summary>
        public WriteOffCollection CompletedWork
        {
            get => _completedWork;
            set => Set(ref _completedWork, value);
        }

        /// <summary>
        ///     Строка подключения к TFS
        /// </summary>
        public ObservableCollection<string> Connections
        {
            get => _connections;
            set => Set(ref _connections, value);
        }

        /// <summary>
        ///     Список рабочих элементов на мне
        /// </summary>
        public ObservableCollection<int> MyWorkItems
        {
            get => _myWorkItems;
            set => Set(ref _myWorkItems, value);
        }

        /// <summary>
        /// Список правил валидации
        /// </summary>
        public ObservableCollection<IRule> Rules { get => rules; set => Set(ref rules, value); }

        /// <summary>
        ///     Стратегия как выбираем таск для списывания времени
        /// </summary>
        public WroteOffStrategy Strategy
        {
            get => strategy;
            set => SetProperty(ref strategy, value);
        }

        /// <summary>
        /// Путь к файлу логов
        /// </summary>
        public string LogPath { get => logPath; set => Set(ref logPath, value); }

        #endregion

        #region Methods

        public void Dispose()
        {
            if (_changed)
                Write();
        }

        public static Settings Read()
        {
            Settings settings;

            if (File.Exists(_savePath))
            {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_savePath));
            }
            else
            {
                Trace.WriteLine("Creating new settings");

                settings = new Settings();
            }

            settings._changed = false;

            return settings;
        }

        private void Write()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_savePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(_savePath));

            File.WriteAllText(_savePath, JsonConvert.SerializeObject(this, Formatting.Indented));

            Trace.WriteLine("Settings saved");
        }

        private bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                if (storage is INotifyCollectionChanged old) old.CollectionChanged -= OnCollectionChanged;

                if (value is INotifyCollectionChanged added) added.CollectionChanged += OnCollectionChanged;

                // Проверка на совпадение списков
                if (storage is IEnumerable x
                    && value is IEnumerable y
                    && x.IsTermwiseEquals(y))
                {
                    return false;
                }
            }

            return SetProperty(ref storage, value, propertyName);
        }

        protected override bool SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            var result = base.SetProperty(ref storage, value, propertyName);

            if (result) _changed = true;

            return result;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _changed = true;
        }

        #endregion
    }

    public enum WroteOffStrategy
    {
        [Description("Выбираем случайно")] Random,

        [Description("Выбираем сами")] Watch
    }
}