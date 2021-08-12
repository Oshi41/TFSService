using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using TfsAPI.Comparers;
using TfsAPI.Interfaces;

namespace TfsAPI.TFS.Observers
{
    public class WorkItemObserver : ObserverCollectionBase<WorkItem>, IWorkItemObserver
    {
        private readonly IWorkItem _workItemService;

        public WorkItemObserver(IWorkItem workItemService, Action afterTick, IEnumerable<WorkItem> saved, TimeSpan delay) 
            : base(new IdWorkItemComparer(), new PartialWorkItemComparer(), afterTick, saved, delay)
        {
            _workItemService = workItemService;
        }

        public override async Task<IList<WorkItem>> Request()
        {
            var collection = await Task.Run(() => _workItemService.GetMyWorkItems());

            return collection.OfType<WorkItem>().ToList();
        }

        protected override void OnCollectionChanged(CollectionChangeAction action, IList<WorkItem> collection)
        {
            if (action == CollectionChangeAction.Remove)
            {
                collection.ForEach(x => x.SyncToLatest());
            }
            
            ItemsChanged?.Invoke(this, new ItemsChanged(action, collection));
        }

        public event EventHandler<ItemsChanged> ItemsChanged;
    }
}