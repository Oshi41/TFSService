using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Gui.Settings;
using Mvvm;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace Gui.ViewModels
{
    public class StatsViewModel : BindableBase
    {
        private readonly List<WorkItemVm> _origin = new List<WorkItemVm>();
        
        private ObservableCollection<WorkItemVm> _myItems;

        private string _name;

        private int _capacity;

        private int _wroteOff;

        public StatsViewModel(FilterViewModel filter)
        {
            Filter = new FilterViewModel(filter);

            Filter.FilterChanged += OnFilterChanged;
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int Capacity
        {
            get => _capacity;
            set => SetProperty(ref _capacity, value);
        }

        public int WroteOff
        {
            get => _wroteOff;
            set => SetProperty(ref _wroteOff, value);
        }

        public ObservableCollection<WorkItemVm> MyItems
        {
            get => _myItems;
            set => SetProperty(ref _myItems, value);
        }

        public FilterViewModel Filter { get; }

        public async void Refresh(IWriteOff api, IWorkItem itemApi)
        {
            if (api == null)
                throw new Exception(nameof(api));
            
            if (itemApi == null)
                throw new Exception(nameof(itemApi));

            var now = DateTime.Now;

            // using (var settings = Settings.Settings.Read())
            // {
            //     Capacity = settings.Capacity.ByUser
            //         ? settings.Capacity.Hours
            //         : await Task.Run(api.GetCapacity);
            // }

            using (var settings = new WriteOffSettings().Read<WriteOffSettings>())
            {
                Capacity = settings.Capacity.Hours;
            }

            // TFS API requests
            WroteOff = await Task.Run(() => api.GetWriteoffs(now, now).Sum(x => x.Value));
            Name = await Task.Run(() => api.Name);
            var all = await Task.Run(() => itemApi.GetMyWorkItems().Select(x => new WorkItemVm(x)));

            lock (_origin)
            {
                _origin.Clear();
                _origin.AddRange(all);
                Filter.Ignored.Initialize(_origin, itemApi);
            }

            OnFilterChanged();
        }

        private void OnFilterChanged(object sender = null, EventArgs e = null)
        {
            lock (_origin)
            {
                MyItems = new ObservableCollection<WorkItemVm>(_origin.Where(x => Filter.Accepted(x.Item)));

                using (var settings = new ViewSettings().Read<ViewSettings>())
                {
                    settings.MainFilter = Filter;
                }
            }
        }
    }
}