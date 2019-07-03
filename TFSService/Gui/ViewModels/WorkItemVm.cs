﻿using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Gui.ViewModels
{
    /// <summary>
    /// Обёртка для работы с TFS данными
    /// </summary>
    public class WorkItemVm
    {
        public WorkItemVm(WorkItem item)
        {
            Item = item;
        }

        public WorkItem Item { get; }

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