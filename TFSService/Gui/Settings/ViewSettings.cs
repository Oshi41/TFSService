using System.Collections.ObjectModel;
using Gui.Helper;
using Gui.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gui.Settings
{
    public class ViewSettings : SettingsBase
    {
        private ObservableCollection<string> _connections = new ObservableCollection<string>();
        private VisibleMode _viewMode = VisibleMode.Table;
        private FilterViewModel _mainFilter;
        private string _project;

        public override string ConfigName()
        {
            return "view.json";
        }

        public ObservableCollection<string> Connections
        {
            get => _connections;
            set => Set(ref _connections, value);
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public VisibleMode ViewMode
        {
            get => _viewMode;
            set => Set(ref _viewMode, value);
        }

        public FilterViewModel MainFilter
        {
            get => _mainFilter;
            set
            {
                if (!Equals(value, _mainFilter))
                {
                    Set(ref _mainFilter, new FilterViewModel(value));
                }
            }
        }
        
        public string Project
        {
            get => _project;
            set => Set(ref _project, value);
        }
    }
}