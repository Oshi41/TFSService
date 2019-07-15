using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Comparers
{
    /// <summary>
    ///     Сравнивает WorkITem по ID
    /// </summary>
    public class IdWorkItemComparer : IEqualityComparer<WorkItem>
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