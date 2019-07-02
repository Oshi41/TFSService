using Microsoft.Extensions.Caching.Memory;
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
using TfsAPI.TFS.Capacity;

namespace TfsAPI.TFS
{
    public class CapacitySearcher : ICapacitySearcher
    {
        #region Fields

        private TfsTeamProjectCollection _connection;
        private WorkItemStore _itemStore;
        private IIdentityManagementService2 _managementService;
        private TfsTeamService _teamService;
        private WorkHttpClient _workClient;
        private ICommonStructureService4 _structureService;

        #endregion

        #region Properties

        protected WorkItemStore ItemStore => _itemStore ?? (_itemStore = _connection?.GetService<WorkItemStore>());

        private IIdentityManagementService2 ManagementService => _managementService ?? (_managementService = _connection?.GetService<IIdentityManagementService2>());

        private TfsTeamService TeamService => _teamService ?? (_teamService = _connection?.GetService<TfsTeamService>());

        private WorkHttpClient WorkClient => _workClient ?? (_workClient = _connection.GetClient<WorkHttpClient>());

        private ICommonStructureService4 StructureService => _structureService ?? (_structureService = _connection.GetService<ICommonStructureService4>());
        #endregion

        public CapacitySearcher(TfsTeamProjectCollection connection,
            WorkItemStore itemStore = null,
            IIdentityManagementService2 managementService = null,
            TfsTeamService teamService = null,
            WorkHttpClient workClient = null,
            ICommonStructureService4 structureService = null)
        {
            _connection = connection;
            _itemStore = itemStore;
            _managementService = managementService;
            _teamService = teamService;
            _workClient = workClient;
            _structureService = structureService;
        }


        /// <summary>
        /// Глубокий поиск по TFS
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int SearchActualCapacity(string name)
        {
            var now = DateTime.Now;
            var caps = SearchCapacities(name, now, now);

            return caps.Sum(x => x.GetCapacity(name));
        }

        public virtual List<TeamCapacity> SearchCapacities(string name, DateTime start, DateTime end)
        {
            // Получил все команды, где я принимаю участие
            var myTeams = GetAllMyTeams();

            return ItemStore
                .Projects
                .OfType<Microsoft.TeamFoundation.WorkItemTracking.Client.Project>()
                .Select(project =>
                {
                    // Ищу итерацию
                    var iterations = FindIterations(project, start, end);

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
                        .Select(iter => QuerryCapacity(tuple.Project, team, iter))))
                // Ищу там, где есть доступ
                .Where(x => x != null)
                .AsParallel()
                // Я там должен участвовать
                .Where(x => x.Contains(name))
                .ToList();
        }

        #region Private

        /// <summary>
        /// Глубокий поиск по TFS. Возможно, займет кучу ресурсов. TODO Оптимизовать
        /// </summary>
        /// <returns></returns>
        public IList<TeamFoundationTeam> GetAllMyTeams()
        {
            return ItemStore
                // Проъожу по всем проектам
                .Projects
                .OfType<Microsoft.TeamFoundation.WorkItemTracking.Client.Project>()
                // Вытаскиваю у каждого список команд
                .SelectMany(x => ManagementService.ListApplicationGroups(x.Uri.ToString(), ReadIdentityOptions.ExtendedProperties))
                // Проверяю вхождение в эту группу
                .Where(x => ManagementService.IsMember(x.Descriptor, _connection.AuthorizedIdentity.Descriptor))
                // прочел команду из GUID
                .Select(x => TeamService.ReadTeam(x.TeamFoundationId, null))
                .Where(x => x != null)
                .ToList();
        }

        private TeamCapacity QuerryCapacity(Microsoft.TeamFoundation.WorkItemTracking.Client.Project project, TeamFoundationTeam team, Iteration iter)
        {
            if (project == null || team == null || iter == null)
                return null;

            var members = QuerryCapacity(project.Guid, team.Identity.TeamFoundationId, iter.Id);
            if (members == null)
            {
                return null;
            }

            return new TeamCapacity(project, team, iter, members);

        }

        /// <summary>
        /// Ищем среди всех итераций проекта подходящие по дате
        /// </summary>
        /// <param name="project"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        protected List<Iteration> FindIterations(Microsoft.TeamFoundation.WorkItemTracking.Client.Project project, DateTime start, DateTime end)
        {
            try
            {
                var iters = project
                            .IterationRootNodes
                            .OfType<Node>()
                            .Select(x => StructureService.GetNode(x.Uri.AbsoluteUri))
                            .AsParallel()
                            .ToList();

                return iters
                       .Where(x => x.InRange(start, end))
                       .Select(x => new Iteration(x))
                       .ToList();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Возвращаю список участников проекта с их хар-ками. В случае ошибки возвращаю null
        /// </summary>
        /// <param name="project">GUID Проект</param>
        /// <param name="teamId">GUID команды</param>
        /// <param name="iteration">GUID итерации</param>
        /// <returns></returns>
        private List<TeamMemberCapacity> QuerryCapacity(Guid? project, Guid? teamId, Guid? iteration)
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
                var resp = webReq.GetResponse() as HttpWebResponse;
                using (var reader = new StreamReader(resp.GetResponseStream()))
                {
                    var result = reader.ReadToEnd();

                    var parsed = Parse(result);

                    return parsed;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return null;
            }
        }

        #endregion

        #region public static

        public static List<TeamMemberCapacity> Parse(string jsonRaw)
        {
            var json = JObject.Parse(jsonRaw);

            var members = json["value"];

            var result = JsonConvert.DeserializeObject<List<TeamMemberCapacity>>(members.ToString());
            return result;
        }

        #endregion       
    }

    class CachedCapacitySearcher : CapacitySearcher
    {
        private readonly MemoryCache _cache;

        public CachedCapacitySearcher(
            MemoryCache cache,
            TfsTeamProjectCollection connection,
            WorkItemStore itemStore = null,
            IIdentityManagementService2 managementService = null,
            TfsTeamService teamService = null,
            WorkHttpClient workClient = null,
            ICommonStructureService4 structureService = null)
            : base(connection, itemStore, managementService, teamService, workClient, structureService)
        {
            _cache = cache;
        }

        private const string capacityKey = "capacityKey";
        public override List<TeamCapacity> SearchCapacities(string name, DateTime start, DateTime end)
        {
            var changed = false;

            // Смог вытащить кэшированные записи
            if (_cache.TryGetValue<List<TeamCapacity>>(capacityKey, out var result))
            {
                // скопировал в локальную переменную
                var i = start;

                // Прохожу по всем дням, должен найти каждую итерацию
                while (i <= end)
                {
                    // Не нашли итерацию, делаем запрос
                    if (!result.Any(x => x.Iteration.InRange(i, i)))
                    {
                        var items = base.SearchCapacities(name, i, i);
                        if (items.Any())
                        {
                            result.AddRange(items);
                            changed = true;
                        }                        
                    }

                    i = i.AddDays(1);
                }
            }
            else
            {
                // ищу впервые
                result = base.SearchCapacities(name, start, end);
                changed = true;
            }

            // Были изменения
            if (changed)
            {
                var options = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromDays(1));                

                _cache.Set(capacityKey, result);
            }            
            
            // На выход идут только те, которые попали в указанный предел
            return result.Where(x => x.Iteration.InRange(start, end)).ToList();
        }
    }    
}
