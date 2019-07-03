using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsAPI.TFS.Capacity
{
    /// <summary>
    /// Представление проекта в TFS
    /// </summary>
    public class Project
    {
        public Guid Id { get; }

        public Project(Guid id)
        {
            Id = id;
        }

        public Project(Microsoft.TeamFoundation.WorkItemTracking.Client.Project p)
        {

        }

        public static explicit operator Project(Microsoft.TeamFoundation.WorkItemTracking.Client.Project p)
        {
            return new Project(p);
        }

    }
}
