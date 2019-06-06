using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;
using System.Linq;
using TfsAPI.Extentions;

namespace Gui.ViewModels.Notifications
{
    public class ItemsAssignedBaloonViewModel : BindableNotificationBase
    {
        public ItemsAssignedBaloonViewModel(List<WorkItem> e, 
            string title = "Назначены новые рабочие элементы") 
            : base(title)
        {
            Items = e.Select(x => new WorkItemVm(x)).ToList();            
        }

        public List<WorkItemVm> Items { get; private set; }
    }
}
