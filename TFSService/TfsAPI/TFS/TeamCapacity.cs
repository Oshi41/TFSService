using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsAPI.TFS
{
    public class TeamCapacity
    {
        public Project Project { get;  }
        public TeamSettingsIteration Iteration { get;  }

        public TeamFoundationTeam Team { get;  }

        public List<TeamMemberCapacity> TeamMembers { get;  }

        public TeamCapacity(Project p, TeamFoundationTeam t, TeamSettingsIteration i, IEnumerable<TeamMemberCapacity> m)
        {
            TeamMembers = m.ToList();
            Team = t;
            Iteration = i;
            Project = p;
        }

        /// <summary>
        /// Данный пользователь есть в команде
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            return Find(name).Any();
        }

        /// <summary>
        /// Возвращает списко
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetCapacity(string name)
        {
            var activities = Find(name).SelectMany(x => x.Activities);
            // Не смог найти трудозатраты для пользователя в данной итерации
            if (activities.IsNullOrEmpty())
                return -1;

            return (int)activities.Sum(x => x.CapacityPerDay);
        }

        private List<TeamMemberCapacity> Find(string name)
        {
            // Уточнить формат DisplayName для членов команды.
            // Пока что это работает
            return TeamMembers.Where(x => x.TeamMember.DisplayName.Contains(name)).ToList();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TeamCapacity c))
                return false;

            return Equals(Project?.Guid, c.Project?.Guid)
                && Equals(Team?.Identity?.TeamFoundationId, c.Team?.Identity?.TeamFoundationId)
                && Equals(Iteration?.Id, c.Iteration?.Id);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
