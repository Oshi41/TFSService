using System;
using System.Collections.ObjectModel;
using Microsoft.TeamFoundation.Build.WebApi;
using Newtonsoft.Json;

namespace Gui.Settings
{
    public class BuildQueueSettings : SettingsBase
    {
        private ObservableCollection<Build> _queuedBuilds = new ObservableCollection<Build>();
        private TimeSpan _delay = TimeSpan.FromSeconds(30);

        public ObservableCollection<Build> QueuedBuilds
        {
            get => _queuedBuilds;
            set => Set(ref _queuedBuilds, value);
        }

        public TimeSpan Delay
        {
            get => _delay;
            set => Set(ref _delay, value);
        }

        public override string ConfigName()
        {
            return "buildQueue.json";
        }
    }
}