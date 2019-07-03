using Microsoft.TeamFoundation.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsAPI.TFS.Capacity
{
    /// <summary>
    /// Представление команды в TFS
    /// </summary>
    public class Team
    {
        public Guid Id { get; }

        public Team(Guid id)
        {
            Id = id;
        }

        public Team(TeamFoundationTeam team)
            : this(team.Identity.TeamFoundationId)
        {

        }
    }
}
