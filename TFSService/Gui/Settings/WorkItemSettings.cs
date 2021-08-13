using System;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.VersionControl.Common.Internal;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Gui.Settings
{
    public class WorkItemSettings : SettingsBase
    {
        private bool _observe;
        private ObservableCollection<WorkItem> _items;
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
        /// Рабочие элементы, за которыми наблюдаем
        /// </summary>
        public ObservableCollection<WorkItem> Items
        {
            get => _items;
            set => Set(ref _items, value);
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
            return "workitems.json";
        }
    }
}