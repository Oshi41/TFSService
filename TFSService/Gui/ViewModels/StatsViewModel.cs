using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.VersionControl.Common.Internal;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsAPI.Interfaces;

namespace Gui.ViewModels
{
    public class StatsViewModel : BindableBase
    {
        private string _name;
        private int capacity;
        private int _tfsCapacity;
        private int wroteOff;
        private ObservableCollection<WorkItemVm> myItems;        

        public string Name { get => _name; set => SetProperty(ref _name, value); }

        public int Capacity { get => capacity; set => SetProperty(ref capacity, value); }

        public int TfsCapacity { get => _tfsCapacity; set => SetProperty(ref _tfsCapacity, value); }

        public int WroteOff { get => wroteOff; set => SetProperty(ref wroteOff, value); }

        public ObservableCollection<WorkItemVm> MyItems { get => myItems; set => SetProperty(ref myItems, value); }

        public void Refresh(ITfsApi api)
        {
            if (api == null)
                throw new Exception(nameof(api));

            var now = DateTime.Now;

            Name = api.Name;
            TfsCapacity = api.GetCapacity();
            WroteOff = api.GetCheckins(now, now).Sum(x => x.Value);

            MyItems = new ObservableCollection<WorkItemVm>(api.GetMyWorkItems().Select(x => new WorkItemVm(x)));

            Capacity = Settings.Settings.Read().Capacity;
        }
    }
}
