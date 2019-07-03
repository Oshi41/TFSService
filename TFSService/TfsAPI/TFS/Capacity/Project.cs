using System;

namespace TfsAPI.TFS.Capacity
{
    /// <summary>
    ///     Представление проекта в TFS
    /// </summary>
    public class Project
    {
        public Project(Guid id)
        {
            Id = id;
        }

        public Project(Microsoft.TeamFoundation.WorkItemTracking.Client.Project p)
        {
        }

        public Guid Id { get; }

        public static explicit operator Project(Microsoft.TeamFoundation.WorkItemTracking.Client.Project p)
        {
            return new Project(p);
        }
    }
}