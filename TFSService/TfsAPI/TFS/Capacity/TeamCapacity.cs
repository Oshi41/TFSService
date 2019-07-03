using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Work.WebApi;
using TfsAPI.TFS.Capacity;
using Project = Microsoft.TeamFoundation.WorkItemTracking.Client.Project;

namespace TfsAPI.TFS
{
    /// <summary>
    ///     Трудозатраты команды. Нельзя использовать в HashSet
    /// </summary>
    public class TeamCapacity
    {
        public TeamCapacity(Project p, TeamFoundationTeam t, TeamSettingsIteration i, IEnumerable<TeamMemberCapacity> m)
            : this(new Capacity.Project(p), new Team(t), new Iteration(i), m)
        {
        }

        public TeamCapacity(Project p, TeamFoundationTeam t, Iteration i, IEnumerable<TeamMemberCapacity> m)
            : this(new Capacity.Project(p), new Team(t), i, m)
        {
        }

        public TeamCapacity(Capacity.Project p, Team t, Iteration i, IEnumerable<TeamMemberCapacity> members)
        {
            Project = p;
            Team = t;
            Iteration = i;
            TeamMembers = members.ToList();
        }

        /// <summary>
        ///     Проект
        /// </summary>
        public Capacity.Project Project { get; }

        /// <summary>
        ///     Итерация
        /// </summary>
        public Iteration Iteration { get; }

        /// <summary>
        ///     Команды
        /// </summary>
        public Team Team { get; }

        /// <summary>
        ///     Члены команды
        /// </summary>
        public List<TeamMemberCapacity> TeamMembers { get; }

        /// <summary>
        ///     Данный пользователь есть в команде
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            return Find(name).Any();
        }

        /// <summary>
        ///     Возвращает списко
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetCapacity(string name)
        {
            var activities = Find(name).SelectMany(x => x.Activities);
            // Не смог найти трудозатраты для пользователя в данной итерации
            if (activities.IsNullOrEmpty())
                return -1;

            return (int) activities.Sum(x => x.CapacityPerDay);
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

            return Equals(Project?.Id, c.Project?.Id)
                   && Equals(Team?.Id, c.Team?.Id)
                   && Equals(Iteration?.Id, c.Iteration?.Id);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}