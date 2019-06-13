using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Interfaces;

namespace TfsAPI.Rules
{
    public abstract class MyItemsRule : IRule
    {
        protected readonly ITfsApi _api;
        protected List<WorkItem> _inconsistened;

        protected MyItemsRule(ITfsApi api)
        {
            _api = api;
        }


        #region Implementation of IRule

        public bool Passed()
        {
            var myItems = _api.GetMyWorkItems();
            var second = _api.QueryItems(GetQuery());

            _inconsistened = GetInconsistent(myItems, second);

            return _inconsistened.IsNullOrEmpty();
        }

        public List<WorkItem> GetInconsistent()
        {
            return _inconsistened;
        }

        #endregion

        protected abstract string GetQuery();
        protected abstract List<WorkItem> GetInconsistent(IList<WorkItem> myItems, IList<WorkItem> queried);
    }
}
