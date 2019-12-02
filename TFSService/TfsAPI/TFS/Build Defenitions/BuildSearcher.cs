using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TfsAPI.Logger;

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
            BuildResult? result = null,
            string actor = null)
        {
            if (finish == null) finish = DateTime.Now;

            if (start == null) start = finish.Value.AddDays(-10);

            var builds = new List<Build>();

            foreach (var x in _myProjects)
                try
                {
                    var projectBuilds = _client.GetBuildsAsync(x,
                            minFinishTime: start,
                            maxFinishTime: finish,
                            resultFilter: result,
                            requestedFor: actor)
                        .Result;

                    builds.AddRange(projectBuilds);
                }
                catch (AggregateException e)
                    when (e.InnerException is VssServiceResponseException ex)
                {
                    LoggerHelper.WriteLine($" Not enough privileges");
                }
                catch (Exception e)
                {
                    LoggerHelper.WriteLine(e);
                }

            return builds;
        }
    }
}