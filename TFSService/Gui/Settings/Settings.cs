using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Gui.Helper;
using Mvvm;
using Newtonsoft.Json;
using TfsAPI.Attributes;
using TfsAPI.Extentions;
using TfsAPI.RulesNew;

namespace Gui.Settings
{
    /// <summary>
    /// Класс хранения настроек приложения. При Dispose пытается сохраниться в файл, если были произведены изменения
    /// </summary>
    public class Settings : BindableBase, IDisposable
    {
        public Settings()
        {
            CompletedWork = new WriteOffCollection();
            Connections = new ObservableCollection<string>();
            MyWorkItems = new ObservableCollection<int>();
            Rules = new ObservableCollection<IRule>();
            Capacity = new Capacity();
        }

        #region Fields

        private static readonly string _savePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TfsService",
            "config.json");

        private static JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
        };

        private Capacity _capacity;
        private TimeSpan _duration;
        private DateTime _begin;

        private bool _changed;
        private ObservableCollection<string> _connections;
        private WriteOffCollection _completedWork;
        private ObservableCollection<int> _myWorkItems;
        private WroteOffStrategy strategy;
        private string logPath = Path.Combine(Path.GetDirectoryName(_savePath), "logs.log");
        private ObservableCollection<IRule> rules;
        private int itemMinutesCheck = 5;
        private int oldReviewDay = 100;

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
        public Capacity Capacity
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
        [JsonConverter(typeof(JCollectionConverter<IRule, Rule>))]
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

        /// <summary>
        /// Время между обновлениями моих рабочих элементов в минутах
        /// </summary>
        public int ItemMinutesCheck { get => itemMinutesCheck; set => Set(ref itemMinutesCheck, value); }

        /// <summary>
        /// Период дней, после которого проверка кодя является устаревшей
        /// </summary>
        public int OldReviewDay { get => oldReviewDay; set => Set(ref oldReviewDay, value); }

        #endregion

        #region Methods

        /// <summary>
        /// Пытаемся сохраниться, если были изменения
        /// </summary>
        public void Dispose()
        {
            if (_changed)
                Write();
        }

        /// <summary>
        /// Читаем настройки из файла, либо берем дефолтные
        /// </summary>
        /// <returns></returns>
        public static Settings Read()
        {
            Settings settings;

            if (File.Exists(_savePath))
            {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_savePath), _settings);
            }
            else
            {
                Trace.WriteLine($"{nameof(Settings)}.{nameof(Read)}:Creating new settings");

                settings = new Settings();
            }

            settings._changed = false;

            return settings;
        }

        /// <summary>
        /// Записываем настройки в json формат в файл
        /// </summary>
        private void Write()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_savePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(_savePath));

            File.WriteAllText(_savePath, JsonConvert.SerializeObject(this, _settings));

            Trace.WriteLine($"{nameof(Settings)}.{nameof(Write)}: Settings saved");
        }

        /// <summary>
        /// Изменяем свойство новым значением. 
        /// <para>Если свойство <see cref="INotifyCollectionChanged"/> либо <see cref="INotifyPropertyChanged"/>, 
        /// наблюдаем за его изменениями, подписываясь на события</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                if (storage is INotifyCollectionChanged old) old.CollectionChanged -= NotifyChanges;

                if (value is INotifyCollectionChanged added) added.CollectionChanged += NotifyChanges;

                // Проверка на совпадение списков
                if (storage is IEnumerable x
                    && value is IEnumerable y
                    && x.IsTermwiseEquals(y))
                {
                    return false;
                }

                if (storage is INotifyPropertyChanged prev) prev.PropertyChanged -= NotifyChanges;
                if (value is INotifyPropertyChanged newVal) newVal.PropertyChanged += NotifyChanges;
            }

            return SetProperty(ref storage, value, propertyName);
        }

        private void NotifyChanges(object sender, EventArgs e)
        {
            _changed = true;
        }

        /// <summary>
        /// Ппри каждом значащем изменении, взводим флажок наличия изменений
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected override bool SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            var result = base.SetProperty(ref storage, value, propertyName);

            if (result) _changed = true;

            return result;
        }

        #endregion
    }

    /// <summary>
    /// Стратегия выбора рабочего элемента для списания времени
    /// </summary>
    public enum WroteOffStrategy
    {
        [LocalizedDescription(nameof(Properties.Resources.AS_ChooseRandomly), typeof(Properties.Resources))] Random,

        [LocalizedDescription(nameof(Properties.Resources.AS_ChooseByMyOwn), typeof(Properties.Resources))] Watch
    }
}