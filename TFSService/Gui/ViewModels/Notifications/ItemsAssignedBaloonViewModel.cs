using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm.Commands;

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
            OpenLinkCommand = new DelegateCommand(OpenLink, CanOpenLink);
        }        

        public List<WorkItemVm> Items { get; }

        public WorkItemVm Selected { get => _selected; set => Set(ref _selected, value); }

        public ICommand OpenLinkCommand { get;  }

        private void OpenLink()
        {
            var link = Selected?.Item?.Uri?.ToString();

            if (link != null)
            {
                // System.Diagnostics.Process.Start(link);
            }
        }

        private bool CanOpenLink()
        {
            return Selected != null;
        }
    }
}