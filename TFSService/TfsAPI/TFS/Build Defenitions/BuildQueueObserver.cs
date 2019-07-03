using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TfsAPI.TFS.Build_Defenitions
{
    public class BuildQueueObserver
    {
        private readonly IBuildServer _buildServer;
        private readonly string _name;
        private readonly BuildHttpClient _client;
        private readonly Guid[] _myProjects;

        public BuildQueueObserver(string name, BuildHttpClient client, IBuildServer buildServer, params Guid[] myProjects)
        {
            _name = name;
            _client = client;
            _buildServer = buildServer;
            _myProjects = myProjects;
        }

        public IList<Build> FindBuilds(DateTime? start = null,
            DateTime? finish = null,
            BuildResult? result = null,
            Func<Build, bool> specialPredicat = null)
        {
            if (finish == null)
            {
                finish = DateTime.Now;
            }

            if (start == null)
            {
                start = finish.Value.AddDays(-10);
            }

            if (specialPredicat == null)
            {
                specialPredicat = o => true;
            }

            var builds = _myProjects
                .SelectMany(x => _client
                             .GetBuildsAsync2(
                                    x,
                                    minFinishTime: start,
                                    maxFinishTime: finish,
                                    resultFilter: result)
                             .Result)
                .AsParallel()
                .Where(x => specialPredicat(x))
                .ToList();

            return builds;
        }

        public void FindQueuedBuilds()
        {
            var agents = _buildServer.QueryBuildAgents(_buildServer.CreateBuildAgentSpec());
            
            if (agents?.Agents.IsNullOrEmpty() != false)
            {
                return;
            }
        }

    }
}
