using Microsoft.IdentityModel.Tokens;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TfsAPI.Extentions;

namespace TfsAPI.TFS
{
    public class CapacitySearcher
    {
        #region Fields

        private TfsTeamProjectCollection _connection;
        private WorkItemStore _itemStore;
        private IIdentityManagementService2 _managementService;
        private TfsTeamService _teamService;
        private WorkHttpClient _workClient;

        #endregion

        #region Properties

        private WorkItemStore ItemStore => _itemStore ?? (_itemStore = _connection?.GetService<WorkItemStore>());

        private IIdentityManagementService2 ManagementService => _managementService ?? (_managementService = _connection?.GetService<IIdentityManagementService2>());

        private TfsTeamService TeamService => _teamService ?? (_teamService = _connection?.GetService<TfsTeamService>());

        private WorkHttpClient WorkClient => _workClient ?? (_workClient = _connection.GetClient<WorkHttpClient>());

        #endregion

        public CapacitySearcher(TfsTeamProjectCollection connection,
            WorkItemStore itemStore = null,
            IIdentityManagementService2 managementService = null,
            TfsTeamService teamService = null,
            WorkHttpClient workClient = null)
        {
            _connection = connection;
            _itemStore = itemStore;
            _managementService = managementService;
            _teamService = teamService;
            _workClient = workClient;
        }


        /// <summary>
        /// Глубокий поиск по TFS
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int SearchActualCapacity(string name)
        {
            var teamCapacities = DeepCapacitySearch()
                .SelectMany(x => x)
                .AsParallel();

            var searched = teamCapacities.ToList();

            var mine = searched
                .Where(x => x.TeamMember.DisplayName.Contains(name)
                    && x.Activities.Sum(y => y.CapacityPerDay) > 0).ToList();

            return (int)mine.Sum(x => x.Activities.Sum(j => j.CapacityPerDay));
        }

        public List<TeamCapacity> SearchCapacities(string name, DateTime start, DateTime end)
        {
            // Получил все команды, где я принимаю участие
            var myTeams = GetAllMyTeams();

            return ItemStore
                .Projects
                .OfType<Project>()
                .Select(project =>
                {
                    // Ищу итерацию
                    var iterations = FindIterations(project, start, end).Result;
                    
                    // Не смог найти ни одной итерации, смысла в этом проекте нет возвращаю null
                    if (iterations.IsNullOrEmpty())
                        return null;

                    return new { Iterations = iterations, Project = project };
                })
                // Ищу там, где есть доступ
                .Where(x => x != null)
                // Прохожу по всем командам
                .SelectMany(tuple => myTeams
                    .SelectMany(team => tuple
                        // Прохожу по всем итерациям
                        .Iterations
                        // Запрашиваю трудозатраты всей команды
                        .Select(iter => QuerryCapacity(tuple.Project, team, iter).Result)))
                .AsParallel()
                // Я там должен участвовать
                .Where(x => x.Contains(name))
                .ToList();
        }

        #region Private

        private IEnumerable<List<TeamMemberCapacity>> DeepCapacitySearch()
        {
            // Получил все команды, где я принимаю участие
            var myTeams = GetAllMyTeams();

            foreach (Project project in ItemStore.Projects)
            {
                var currentIteration = GetCurrentIteration(project).Result;
                if (currentIteration == null)
                    continue;


                foreach (var team in myTeams)
                {
                    var projId = project?.Guid;
                    var teamid = team?.Identity?.TeamFoundationId;
                    var iterId = currentIteration?.Id;

                    yield return QuerryCapacity(projId, teamid, iterId).Result;
                }
            }
        }

        /// <summary>
        /// Глубокий поиск по TFS. Возможно, займет кучу ресурсов. TODO Оптимизовать
        /// </summary>
        /// <returns></returns>
        private IList<TeamFoundationTeam> GetAllMyTeams()
        {
            return ItemStore
                // Проъожу по всем проектам
                .Projects
                .OfType<Project>()
                // Вытаскиваю у каждого список команд
                .SelectMany(x => ManagementService.ListApplicationGroups(x.Uri.ToString(), ReadIdentityOptions.ExtendedProperties))
                // Проверяю вхождение в эту группу
                .Where(x => ManagementService.IsMember(x.Descriptor, _connection.AuthorizedIdentity.Descriptor))
                // прочел команду из GUID
                .Select(x => TeamService.ReadTeam(x.TeamFoundationId, null))
                .Where(x => x != null)
                .ToList();
        }

        /// <summary>
        /// Получаю список итерация проекта.
        /// <para>Если доступа нет, возвращаю null</para>
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private async Task<TeamSettingsIteration> GetCurrentIteration(Project project)
        {
            try
            {
                var context = new Microsoft.TeamFoundation.Core.WebApi.Types.TeamContext(project.Guid);
                var iterations = await WorkClient.GetTeamIterationsAsync(context, "current");
                return iterations.FirstOrDefault();
            }
            catch (Exception e)
            {
                // possibly do not have access
                Trace.WriteLine(e);
                return null;
            }
        }

        private async Task<List<TeamSettingsIteration>> FindIterations(Project project, DateTime start, DateTime end)
        {
            try
            {
                var context = new Microsoft.TeamFoundation.Core.WebApi.Types.TeamContext(project.Guid);
                var iterations = await WorkClient.GetTeamIterationsAsync(context);

                return iterations.Where(x => x.InRange(start, end)).ToList();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return null;
            }
        }

        private async Task<TeamCapacity> QuerryCapacity(Project project, TeamFoundationTeam team, TeamSettingsIteration currentIteration)
        {
            if (project == null || team == null || currentIteration == null)
                return null;

            return new TeamCapacity(project, team, currentIteration, await QuerryCapacity(project.Guid, team.Identity.TeamFoundationId, currentIteration.Id));
        }

        /// <summary>
        /// Возвращаю список участников проекта с их хар-ками
        /// </summary>
        /// <param name="project">GUID Проект</param>
        /// <param name="teamId">GUID команды</param>
        /// <param name="iteration">GUID итерации</param>
        /// <returns></returns>
        private async Task<List<TeamMemberCapacity>> QuerryCapacity(Guid? project, Guid? teamId, Guid? iteration)
        {
            if (!project.HasValue || !teamId.HasValue || !iteration.HasValue)
            {
                return new List<TeamMemberCapacity>();
            }

            var request = $"{_connection?.Uri?.ToString()}/{project}/{teamId}/_apis/work/teamsettings/iterations/{iteration}/capacities";

            var webReq = WebRequest.CreateHttp(request) as HttpWebRequest;

            webReq.Method = "GET";
            webReq.Credentials = _connection.Credentials;
            webReq.ContentType = "text/html";

            // FederatedCookieHelper.EnsureFederatedIdentityCookies(teamProjectCollection, httpWebRequest);

            try
            {
                var resp = await webReq.GetResponseAsync() as HttpWebResponse;
                using (var reader = new StreamReader(resp.GetResponseStream()))
                {
                    var result = await reader.ReadToEndAsync();

                    var parsed = Parse(result);

                    return parsed;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return new List<TeamMemberCapacity>();
            }
        }

        public static List<TeamMemberCapacity> Parse(string jsonRaw)
        {
            var json = JObject.Parse(jsonRaw);

            var members = json["value"];

            var result = JsonConvert.DeserializeObject<List<TeamMemberCapacity>>(members.ToString());
            return result;
        }

        #endregion
    }
}
