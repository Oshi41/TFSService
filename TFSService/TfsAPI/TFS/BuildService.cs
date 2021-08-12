using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using TfsAPI.Interfaces;
using TfsAPI.Logger;

namespace TfsAPI.TFS
{
    public class BuildService : IBuild
    {
        private readonly IConnect _connectService;
        private readonly List<Build> _scheduled;
        private readonly BuildHttpClient _buildClient;

        public BuildService(IConnect connectService, IEnumerable<Build> scheduled)
        {
            _connectService = connectService;
            _scheduled = scheduled?.ToList() ?? new List<Build>();

            _buildClient = _connectService.Tfs.GetClient<BuildHttpClient>();
        }


        public ReadOnlyCollection<Build> GetQueue()
        {
            return new ReadOnlyCollection<Build>(_scheduled);
        }

        public async Task<IList<Build>> GetRunningBuilds()
        {
            var status = BuildStatus.Cancelling | BuildStatus.InProgress | BuildStatus.NotStarted;

            var result = await _buildClient.GetBuildsAsync(_connectService.Project.Name, statusFilter: status);
            return result;
        }

        public async Task<IList<BuildDefinitionReference>> GetAllDefentitions()
        {
            var defs = await _buildClient.GetDefinitionsAsync(
                _connectService.Project.Name,
                queryOrder:DefinitionQueryOrder.LastModifiedDescending,
                builtAfter:DateTime.Now.AddMonths(-6)
                );
            return defs;
        }

        public async Task<IDictionary<string, BuildDefinitionVariable>> GetDefaultProperties(
            BuildDefinitionReference def)
        {
            var buildDef = (await _buildClient.GetFullDefinitionsAsync(project: _connectService.Project.Name, def.Name))
                ?.FirstOrDefault();
            return buildDef?.Variables;
        }

        public Task<Build> Queue(Build build)
        {
            return _buildClient.QueueBuildAsync(build);
        }

        public async Task Tick()
        {
            if (!_scheduled.Any())
            {
                return;
            }

            var builds = await GetRunningBuilds();

            if (builds.Any())
            {
                return;
            }

            LoggerHelper.WriteLine("No active builds");

            var first = _scheduled[0];
            var build = await Queue(first);

            if (build != null && build.Status == BuildStatus.InProgress)
            {
                _scheduled.Remove(first);
                LoggerHelper.WriteLine(
                    $"Build was scheduled for {build.Definition.Name}, {_scheduled.Count} build(s) in a queue");
            }
        }

        public async Task<Build> Schedule(string project, string defName,
            IDictionary<string, BuildDefinitionVariable> properties, bool forced)
        {
            if (string.IsNullOrEmpty(project))
            {
                throw new ArgumentException("No project specified", nameof(project));
            }

            if (string.IsNullOrEmpty(defName))
            {
                throw new ArgumentException("No definition specified", nameof(defName));
            }

            var build = _scheduled.FirstOrDefault(x => x?.Project?.Name == project);

            if (build != null)
            {
                LoggerHelper.WriteLine($"Replacing old build for {build.Project.Name}");
                _scheduled.Remove(build);
            }

            var projectObj = _connectService.WorkItemStore.Projects[project];
            if (projectObj == null)
            {
                throw new Exception($"Project {project} cannot be found");
            }

            var definition = (await _buildClient.GetDefinitionsAsync(project, defName)).FirstOrDefault();
            if (definition == null)
            {
                throw new Exception($"Definition {defName} cannot be found");
            }

            build = new Build
            {
                Project = definition.Project,
                Definition = definition,
                Parameters = "{" + string.Join(",", properties
                    .Select(x => $"\"{x.Key}\":\"{x.Value.Value}\"")) + "}"
            };

            if (forced)
            {
                return await Queue(build);
            }
            else
            {
                _scheduled.Add(build);
            }

            return build;
        }
    }
}