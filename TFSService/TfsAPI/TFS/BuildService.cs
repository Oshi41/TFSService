using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Common;
using TfsAPI.Interfaces;
using TfsAPI.Logger;

namespace TfsAPI.TFS
{
    public class BuildService : IBuild
    {
        private readonly IConnect _connectService;
        private readonly List<Build> _scheduled;
        private readonly BuildHttpClient _buildClient;
        private readonly Timer _timer;

        public BuildService(IConnect connectService, IEnumerable<Build> scheduled, TimeSpan span)
        {
            _connectService = connectService;
            _scheduled = scheduled?.ToList() ?? new List<Build>();

            _buildClient = _connectService.Tfs.GetClient<BuildHttpClient>();
            _timer = new Timer();
            _timer.Interval = span.TotalMilliseconds;
            _timer.Elapsed += async (sender, args) => await Tick();
            
            _timer.Start();
        }


        public ReadOnlyCollection<Build> GetQueue()
        {
            return new ReadOnlyCollection<Build>(_scheduled);
        }

        public async Task<IList<Build>> GetRunningBuilds()
        {
            var status = BuildStatus.Cancelling | BuildStatus.InProgress | BuildStatus.NotStarted;

            LoggerHelper.WriteLine($"Searhing current builds...");
            var result = await _buildClient.GetBuildsAsync(_connectService.Project.Name, statusFilter: status);
            LoggerHelper.WriteLine($"Running {result.Count} builds:");
            LoggerHelper.WriteLine(Environment.NewLine + string.Join(Environment.NewLine, result.Select(x => x?.Definition?.Name)));
            
            return result;
        }

        public async Task<IList<BuildDefinitionReference>> GetAllDefentitions()
        {
            var monthsOld = 6;
            
            LoggerHelper.WriteLine($"Searching all definitions running at least one in {monthsOld} previous months...");
            
            var defs = await _buildClient.GetDefinitionsAsync(
                _connectService.Project.Name,
                queryOrder:DefinitionQueryOrder.LastModifiedDescending,
                builtAfter:DateTime.Now.AddMonths(-monthsOld)
                );
            
            LoggerHelper.WriteLine($"Founded {defs.Count} builds:");
            LoggerHelper.WriteLine(Environment.NewLine + string.Join(Environment.NewLine, defs.Select(x => x?.Name)));
            
            return defs;
        }

        public async Task<IDictionary<BuildDefinitionReference, IDictionary<string, BuildDefinitionVariable>>> GetDefaultVariables(IEnumerable<BuildDefinitionReference> source)
        {
            var references = source.ToDictionary(x => x.Id);
            
            LoggerHelper.WriteLine($"Searching full definition for {references.Count}:");
            LoggerHelper.WriteLine(Environment.NewLine + string.Join(Environment.NewLine, references.Values.Select(x => x.Name)));

            var result = (await _buildClient.GetFullDefinitionsAsync(project: _connectService.Project.Name,
                    definitionIds: references.Keys))
                .ToDictionary(x => references[x.Id], x => x.Variables);
            
            LoggerHelper.WriteLine($"Founded {result.Count}");

            return result;
        }

        public async Task<IList<Build>> Update(IEnumerable<Build> old)
        {
            if (old.IsNullOrEmpty())
                return new List<Build>();
            
            LoggerHelper.WriteLine($"Updating {old.Count()} builds: {Environment.NewLine}{string.Join(Environment.NewLine, old.Select(x => x?.Definition?.Name))}");
            return await _buildClient.GetBuildsAsync(_connectService.Name, buildIds: old.Select(x => x.Id));
        }

        public Task<Build> Queue(Build build)
        {
            LoggerHelper.WriteLine($"Queueing new build {build?.Definition?.Name} with params:");
            LoggerHelper.WriteLine(Environment.NewLine + string.Join(Environment.NewLine, build?.Parameters.Trim('{', '}').Split(',')));
            
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