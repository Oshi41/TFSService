using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TfsAPI.TFS.Build_Defenitions
{
    public class BuildSearcher
    {
        private readonly BuildHttpClient _client;
        private readonly Guid[] _myProjects;

        public BuildSearcher(BuildHttpClient client, params Guid[] myProjects)
        {
            _client = client;
            _myProjects = myProjects;
        }

        public IList<Build> FindCompletedBuilds(DateTime? start = null,
            DateTime? finish = null,
            BuildResult? result = null)
        {
            if (finish == null)
            {
                finish = DateTime.Now;
            }

            if (start == null)
            {
                start = finish.Value.AddDays(-10);
            }

            var builds = new List<Build>();

            foreach (var x in _myProjects)
            {
                try
                {
                    builds.AddRange(_client
                                   .GetBuildsAsync2(x,
                                                    minFinishTime: start,
                                                    maxFinishTime: finish,
                                                    resultFilter: result)
                                   .Result);
                }
                catch (Exception e)
                {
                    Trace.WriteLine($"{nameof(BuildSearcher)}.{nameof(FindCompletedBuilds)}: " + e);
                }
            }

            return builds;

        }

    }
}
