using System;
using System.IO;
using Mvvm;
using Newtonsoft.Json;

namespace Gui.Settings
{
    public class Settings : BindableBase, IDisposable
    {
        #region Fields

        private static string _savePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TfsService", 
            "config.json");

        private string _connection;
        private int _completed;
        private int _capacity;
        private TimeSpan _duration;
        private DateTime _begin;

        private bool _changed;

        #endregion

        public Settings()
        {
            IsDefault = true;
        }

        #region Properties

        /// <summary>
        /// Начало рабочего дня
        /// </summary>
        public DateTime Begin
        {
            get => _begin;
            set => SetProperty(ref _begin, value);
        }

        /// <summary>
        /// Продолжительность рабочего дня
        /// </summary>
        public TimeSpan Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        /// <summary>
        /// Сколько часов надо списать
        /// </summary>
        public int Capacity
        {
            get => _capacity;
            set => SetProperty(ref _capacity, value);
        }

        /// <summary>
        /// Сколько часов уже было списано
        /// </summary>
        public int Completed
        {
            get => _completed;
            set => SetProperty(ref _completed, value);
        }

        /// <summary>
        /// Строка подключения к TFS
        /// </summary>
        public string Connection
        {
            get => _connection;
            set => SetProperty(ref _connection, value);
        }

        [JsonIgnore]
        public bool IsDefault { get; private set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            if (_changed)
                Write();
        }

        public static Settings Read()
        {
            return File.Exists(_savePath)
                ? JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_savePath))
                : new Settings();
        }

        private void Write()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_savePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(_savePath));

            File.WriteAllText(_savePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        protected override bool SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            var result = base.SetProperty(ref storage, value, propertyName);

            if (result)
            {
                IsDefault = false;
                _changed = true;
            }

            return result;
        }

        #endregion
    }
}
