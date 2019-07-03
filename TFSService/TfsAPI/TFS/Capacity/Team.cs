using System;
using Microsoft.TeamFoundation.Client;

namespace TfsAPI.TFS.Capacity
{
    /// <summary>
    ///     Представление команды в TFS
    /// </summary>
    public class Team
    {
        public Team(Guid id)
        {
            Id = id;
        }

        public Team(TeamFoundationTeam team)
            : this(team.Identity.TeamFoundationId)
        {
        }

        public Guid Id { get; }
    }
}