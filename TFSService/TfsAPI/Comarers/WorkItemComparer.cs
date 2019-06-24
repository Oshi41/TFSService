using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Comarers
{
    public class WorkItemComparer : IEqualityComparer<WorkItem>
    {
        public bool Equals(WorkItem x, WorkItem y)
        {
            return x?.Id == y?.Id;
        }

        public int GetHashCode(WorkItem obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}