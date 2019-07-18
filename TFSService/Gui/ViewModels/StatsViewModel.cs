using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Mvvm;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace Gui.ViewModels
{
    public class StatsViewModel : BindableBase
    {
        private readonly List<WorkItemVm> _origin = new List<WorkItemVm>();
        private int _capacity;
        private ObservableCollection<WorkItemVm> _myItems;

        private string _name;
        private int _tfsCapacity;
        private int _wroteOff;

        public StatsViewModel()
        {
            Filter = new FilterViewModel(WorkItemTypes.Task,
                WorkItemTypes.Pbi,
                WorkItemTypes.Bug,
                WorkItemTypes.Improvement,
                WorkItemTypes.Incident,
                WorkItemTypes.Feature);

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

        public int TfsCapacity
        {
            get => _tfsCapacity;
            set => SetProperty(ref _tfsCapacity, value);
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

        public async void Refresh(ITfsApi api)
        {
            if (api == null)
                throw new Exception(nameof(api));

            var now = DateTime.Now;
            Capacity = Settings.Settings.Read().Capacity.Hours;

            // TFS API requests
            TfsCapacity = await Task.Run(() => api.GetCapacity());
            WroteOff = await Task.Run(() => api.GetWriteoffs(now, now).Sum(x => x.Value));
            Name = await Task.Run(() => api.Name);
            var all = await Task.Run(() => api.GetMyWorkItems().Select(x => new WorkItemVm(x)));

            _origin.Clear();
            _origin.AddRange(all.Where(x =>
                !x.Item.IsTypeOf(WorkItemTypes.CodeReview, WorkItemTypes.ReviewResponse)));

            OnFilterChanged();
        }

        private void OnFilterChanged(object sender = null, EventArgs e = null)
        {
            var types = Filter.GetSelectedTypes();

            MyItems = new ObservableCollection<WorkItemVm>(_origin
                .Where(x => x.Item.IsTypeOf(types)));
        }
    }
}