using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Gui.ViewModels.Notifications
{
    public class ItemsAssignedBaloonViewModel : BindableNotificationBase
    {
        private WorkItemVm _selected;

        public ItemsAssignedBaloonViewModel(IEnumerable<WorkItem> e,
            string title = null)
            : base(title ?? Properties.Resources.AS_NewItemsAssigned)
        {
            Items = e.Select(x => new WorkItemVm(x)).ToList();

            Options.DoubleClickAction = sender => OpenLink();
        }

        public List<WorkItemVm> Items { get; }

        public WorkItemVm Selected { get => _selected; set => Set(ref _selected, value); }

        private void OpenLink()
        {
            var link = Selected?.Item?.Uri?.ToString();

            if (link != null)
            {
                System.Diagnostics.Process.Start(link);
            }
        }
    }
}