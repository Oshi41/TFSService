using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;
using System.Linq;

namespace Gui.ViewModels.Notifications
{
    class ItemsAssignedBaloonViewModel : BindableNotificationBase
    {
        public ItemsAssignedBaloonViewModel(List<WorkItem> e) 
            : base("Назначены новые рабочие элементы")
        {
            Items = e.Select(x => new WorkItemVm(x)).ToList();
        }

        public List<WorkItemVm> Items { get; private set; }
    }
}
