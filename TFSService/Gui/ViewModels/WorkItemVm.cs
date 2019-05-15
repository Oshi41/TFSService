using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Gui.ViewModels
{
    public class WorkItemVm
    {
        public WorkItem Item { get; }

        public WorkItemVm(WorkItem item)
        {
            Item = item;
        }

        public override string ToString()
        {
            return Item?.Id.ToString() ?? string.Empty;
        }

        public static implicit operator WorkItemVm(WorkItem item)
        {
            return new WorkItemVm(item);
        }

        public static implicit operator WorkItem(WorkItemVm vm)
        {
            return vm.Item;
        }
    }
}
