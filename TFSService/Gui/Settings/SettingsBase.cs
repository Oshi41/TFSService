using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Mvvm;
using Newtonsoft.Json;
using TfsAPI.Extentions;
using TfsAPI.Logger;

namespace Gui.Settings
{
    public abstract class SettingsBase : BindableBase, IDisposable
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.Indented
        };

        protected bool _wasChanged;

        protected bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                if (storage is INotifyCollectionChanged old) old.CollectionChanged -= NotifyChanges;

                if (value is INotifyCollectionChanged added) added.CollectionChanged += NotifyChanges;

                // Проверка на совпадение списков
                if (storage is IEnumerable x
                    && value is IEnumerable y
                    && x.IsTermwiseEquals(y))
                    return false;

                if (storage is INotifyPropertyChanged prev) prev.PropertyChanged -= NotifyChanges;
                if (value is INotifyPropertyChanged newVal) newVal.PropertyChanged += NotifyChanges;
            }

            return SetProperty(ref storage, value, propertyName);
        }

        protected override bool SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            var wasChanged = base.SetProperty(ref storage, value, propertyName);

            if (wasChanged)
            {
                NotifyChanges();
            }

            return wasChanged;
        }


        private void NotifyChanges(object sender = null, EventArgs e = null)
        {
            _wasChanged = true;
        }

        /// <summary>
        /// Имя конфиг файла
        /// </summary>
        public abstract string ConfigName();

        private string GetPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TfsService",
                ConfigName()
            );
        }

        public void Dispose()
        {
            if (_wasChanged)
            {
                Write();
            }
        }

        public T Read<T>()
            where T : SettingsBase
        {
            try
            {
                if (!File.Exists(GetPath()))
                {
                    Write();
                }

                var text = File.ReadAllText(GetPath());
                JsonConvert.PopulateObject(text, this);

                _wasChanged = false;
            }
            catch (Exception e)
            {
                LoggerHelper.WriteLine(e);
            }

            return this as T;
        }

        public virtual void Write()
        {
            var configPath = GetPath();

            if (!Directory.Exists(Path.GetDirectoryName(configPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(configPath));

            File.WriteAllText(configPath, JsonConvert.SerializeObject(this, JsonSettings));

            LoggerHelper.WriteLine($"JsonSettings saved");
        }
    }
}