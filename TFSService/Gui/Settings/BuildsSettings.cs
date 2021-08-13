using System;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.VersionControl.Common.Internal;

namespace Gui.Settings
{
    public class BuildsSettings : SettingsBase
    {
        private bool _observe;
        private ObservableCollection<Build> _builds;
        private TimeSpan _delay;

        

        /// <summary>
        /// Наблюдаем за изменением сборок?
        /// </summary>
        public bool Observe
        {
            get => _observe;
            set => Set(ref _observe, value);
        }

        /// <summary>
        /// Сборки, за которыми наблюдаем
        /// </summary>
        public ObservableCollection<Build> Builds
        {
            get => _builds;
            set => Set(ref _builds, value);
        }

        /// <summary>
        /// Задержка в обновлении
        /// </summary>
        public TimeSpan Delay
        {
            get => _delay;
            set => Set(ref _delay, value);
        }

        public override string ConfigName()
        {
            return "builds.json";
        }
    }
}