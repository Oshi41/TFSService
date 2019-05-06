using System.Collections.Generic;
using System.Linq;
using Gui.Helper;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI;

namespace Gui.ViewModels
{
    class WorkItemSearcher : BindableExtended
    {
        #region Fields

        private readonly ITfs _tfs;
        private IList<WorkItem> _items;
        private int? _selected;

        private TimedAction<int, WorkItem> _action;

        #endregion

        public WorkItemSearcher(ITfs tfs, bool loadMyItems = false)
        {
            _tfs = tfs;
            _action = new TimedAction<int, WorkItem>(i => _tfs.FindById(i));
            _action.Performed += FillItems;

            Items = loadMyItems
                ? _tfs.GetMyWorkItems()
                : new List<WorkItem>();
        }

        private void FillItems(object sender, WorkItem e)
        {
            var list = new List<WorkItem>();
            if (e != null)
            {
                list.Add(e);
            }

            Items = list;
        }

        public IList<WorkItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public int? Selected
        {
            get => _selected;
            set
            {
                if (SetProperty(ref _selected, value) && Selected.HasValue)
                {
                    PerformSearch();
                }
            }
        }

        private void PerformSearch()
        {
            _action.Shedule(Selected.Value);
        }
    }
}
