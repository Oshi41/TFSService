using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private int _completed;
        private int _capacity;
        private TimeSpan _duration;
        private DateTime _begin;

        private bool _changed;
        private IList<string> _connections;
        private Dictionary<int, int> _completedWork;

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
        /// Сколько часов было списано на разные рабочие элементы
        /// </summary>
        public Dictionary<int, int> CompletedWork
        {
            get => _completedWork;
            set => SetProperty(ref _completedWork, value);
        }

        /// <summary>
        /// Строка подключения к TFS
        /// </summary>
        public IList<string> Connections
        {
            get => _connections;
            set => SetProperty(ref _connections, value);
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
            if (File.Exists(_savePath))
            {
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_savePath));
            }

            Trace.WriteLine("Creating new settings");

            return new Settings();
        }

        private void Write()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_savePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(_savePath));

            File.WriteAllText(_savePath, JsonConvert.SerializeObject(this, Formatting.Indented));

            Trace.WriteLine("Settings saved");
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
