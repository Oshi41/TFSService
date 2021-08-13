using System.Collections.ObjectModel;
using Microsoft.TeamFoundation.Build.WebApi;
using Newtonsoft.Json;

namespace Gui.Settings
{
    public class BuildQueueSettings : SettingsBase
    {
        private ObservableCollection<Build> _queuedBuilds = new ObservableCollection<Build>();

        public ObservableCollection<Build> QueuedBuilds
        {
            get => _queuedBuilds;
            set => Set(ref _queuedBuilds, value);
        }

        public override string ConfigName()
        {
            return "buildQueue.json";
        }
    }
}