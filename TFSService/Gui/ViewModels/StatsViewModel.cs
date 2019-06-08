using System;
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
        private string _name;
        private int _tfsCapacity;
        private int capacity;
        private ObservableCollection<WorkItemVm> myItems;
        private int wroteOff;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int Capacity
        {
            get => capacity;
            set => SetProperty(ref capacity, value);
        }

        public int TfsCapacity
        {
            get => _tfsCapacity;
            set => SetProperty(ref _tfsCapacity, value);
        }

        public int WroteOff
        {
            get => wroteOff;
            set => SetProperty(ref wroteOff, value);
        }

        public ObservableCollection<WorkItemVm> MyItems
        {
            get => myItems;
            set => SetProperty(ref myItems, value);
        }

        public async void Refresh(ITfsApi api)
        {
            if (api == null)
                throw new Exception(nameof(api));

            var now = DateTime.Now;
            Capacity = Settings.Settings.Read().Capacity;

            // TFS API requests
            TfsCapacity = await Task.Run(() => api.GetCapacity());
            WroteOff = await Task.Run(() => api.GetCheckins(now, now).Sum(x => x.Value));
            Name = await Task.Run(() => api.Name);
            var all = await Task.Run(() => api.GetMyWorkItems().Select(x => new WorkItemVm(x)));


            MyItems = new ObservableCollection<WorkItemVm>(all.Where(x =>
                !x.Item.IsTypeOf(WorkItemTypes.CodeReview, WorkItemTypes.ReviewResponse)));
        }
    }
}