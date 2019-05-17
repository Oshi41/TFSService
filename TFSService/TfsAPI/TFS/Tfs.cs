using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.ProcessConfiguration.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;
using Field = Microsoft.TeamFoundation.WorkItemTracking.Client.Field;

namespace TfsAPI.TFS
{
    public class Tfs : ITfs
    {
        #region Fields

        private readonly TfsTeamProjectCollection _project;
        private readonly ILinking _linking;
        private readonly WorkItemStore _itemStore;
        private readonly VersionControlServer _versionControl;
        private readonly TfsTeamService _teamService;
        private TeamSettingsConfigurationService _teamConfiguration;
        private ICommonStructureService4 _structureService;

        private readonly WebHookListener _listener;

        private bool _paused;

        #endregion

        #region Events

        public event EventHandler<CommitCheckinEventArgs> Checkin;

        public event EventHandler<List<WorkItem>> NewItems
        {
            add => _listener.NewItems += value;
            remove => _listener.NewItems -= value;
        }

        public event EventHandler<Dictionary<WorkItem, List<WorkItemEventArgs>>> ItemsChanged
        {
            add => _listener.ItemsChanged += value;
            remove => _listener.ItemsChanged -= value;
        }


        #endregion

        public Tfs(string uri)
        {
            _project = new TfsTeamProjectCollection(new Uri(uri));

            Trace.WriteLine("Connected to " + _project.Name);

            _versionControl = _project.GetService<VersionControlServer>();
            _itemStore = _project.GetService<WorkItemStore>();
            _linking = _project.GetService<ILinking>();
            _teamService = _project.GetService<TfsTeamService>();
            _teamConfiguration = _project.GetService<TeamSettingsConfigurationService>();
            _structureService = _project.GetService<ICommonStructureService4>();
            _listener = new WebHookListener(GetMyWorkItems);
        }

        #region Private methods

        private void Subscribe()
        {
            _versionControl.CommitCheckin += FireCheckinEvent;
            
            _listener.Start();
        }

        private void Unsubscribe()
        {
            _versionControl.CommitCheckin -= FireCheckinEvent;
            Checkin.Unsubscribe();
        }

        #endregion

        #region handlers

        private void FireCheckinEvent(object sender, CommitCheckinEventArgs e)
        {
            if (_paused)
                return;

            Trace.WriteLine($"Chageset ID: {e.ChangesetId}, created by {e.Workspace.DisplayName} at {e.CreationDate:G}");

            Checkin?.Invoke(sender, e);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Unsubscribe();

            _project.Dispose();
            _listener?.Dispose();
        }

        #endregion

        #region ITfs
        
        /// <inheritdoc cref="ITfs.WriteHours"/>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public void WriteHours(WorkItem item, byte hours, bool setActive)
        {
            item.AddHours(hours, setActive);
            item.Save();
            Trace.WriteLine($"From task {item.Id} was writed off {hours} hour(s)");
        }

        /// <inheritdoc cref="ITfs.GetAssociateItems"/>
        public IList<WorkItem> GetAssociateItems(int changeset)
        {
            var result = new List<WorkItem>();

            // Создал объект для получения URL
            var setId = new ArtifactId
            {
                Tool = ToolNames.VersionControl,
                ArtifactType = WorkItemTypes.ChangeSet,
                ToolSpecificId = changeset.ToString(),
            };

            // По этому URL буду искать линкованные элементы
            var uri = LinkingUtilities.EncodeUri(setId);

            // Нашел связи
            var linked = _linking.GetReferencingArtifacts(new[] {uri});

            Trace.WriteLine($"{nameof(GetAssociateItems)}: Founded {linked.Length} links");

            foreach (var artifact in linked)
            {
                // Распарсил url
                var artifactId = LinkingUtilities.DecodeUri(artifact.Uri);
                // Какая-то хитрая проверка
                if (string.Equals(artifactId.Tool, ToolNames.WorkItemTracking, StringComparison.OrdinalIgnoreCase))
                {
                    // Нашёл элемент
                    var item = _itemStore.GetWorkItem(Convert.ToInt32(artifactId.ToolSpecificId));
                    // Добавил
                    result.Add(item);

                }
            }

            Trace.WriteLineIf(result.Any(), $"Changeset {changeset} linked with items: {string.Join(", ", result.Select(x => x.Id))}");

            return result;
        }

        public WorkItem FindById(int id)
        {
            try
            {
                return _itemStore.GetWorkItem(id);
            }
            catch (Exception e)
            {
                Trace.Write(e);
                return null;
            }
        }

        public IList<WorkItem> Search(string text, params string[] allowedTypes)
        {
            var quarry = $"select * from {Sql.Tables.WorkItems} " +
                         $"where ({Sql.Fields.Description} {Sql.ContainsStrOperand} '{text}' " +
                         $"or {Sql.Fields.History} {Sql.ContainsStrOperand} '{text}' " +
                         $"or {Sql.Fields.Title} {Sql.ContainsStrOperand} '{text}' )";

            // Ищу только указанные типы
            if (!allowedTypes.IsNullOrEmpty())
            {
                quarry += " and (" +
                                    $"{string.Join("or ", allowedTypes.Select(x => $"{Sql.Fields.WorkItemType} = '{x}'"))}" +
                                    $")";
            }

            var items = _itemStore.Query(quarry);

            Trace.WriteLine($"Tfs.Search: Founded {items.Count} items");

            return items.OfType<WorkItem>().ToList();
        }

        public int GetCapacity()
        {
            //var workClient = _project.GetClient<WorkHttpClient>();

            //var myTeams = _teamService.QueryTeams(_project.AuthorizedIdentity.Descriptor);
            //var projects = _structureService.ListProjects();

            //foreach (var team in myTeams)
            //{
            //    var config = _teamConfiguration.GetTeamConfigurations(new[] {team.Identity.TeamFoundationId}).FirstOrDefault();
            //    if (config == null)
            //        continue;

            //    foreach (var project in projects)
            //    {
            //        var iterationID = workClient
            //            .GetTeamIterationsAsync(new TeamContext(project.Name))
            //            .Result;

            //        var url = $"{_project.ConfigurationServer.Name}/{team.Name}/{project.Name}" +
            //                  $"/_apis/work/teamsettings/iterations/{iterationID}" +
            //                  "capacities?api-version=4.0";
            //    }
            //}

            //var workClient = _project.GetClient<WorkHttpClient>();

            //foreach (Project project in _itemStore.Projects)
            //{
            //    var iterations = workClient.GetTeamIterationsAsync(new TeamContext(project.Name)).Result;
            //    var current = iterations.FirstOrDefault(x => x.IsCurrent());

            //    var uri = current.Path;
            //}

            //var myTeams = _teamService.QueryTeams(_project.AuthorizedIdentity.Descriptor);
            //int capacity = 0;


            //var configs = _teamConfiguration.GetTeamConfigurations(myTeams.Select(x => x.Identity.TeamFoundationId));

            //foreach (var config in configs)
            //{
            //    var name = config.TeamName;
            //    var iteractions = config.TeamSettings.CurrentIterationPath;
            //}



            //var projects = _itemStore.Projects.OfType<Project>().ToList();
            //int capacity = 0;

            //foreach (var project in projects)
            //{
            //    var team = _teamService.QueryTeams(project.Uri.ToString()).FirstOrDefault();
            //    if (team == null)
            //        continue;

            //    var config = _teamConfiguration.GetTeamConfigurations(new[] {team.Identity.TeamFoundationId})
            //        .FirstOrDefault();

            //    if (config == null)
            //        continue;

            //    foreach (var iteration in config.TeamSettings.IterationPaths)
            //    {
            //        var projectNameIndex = iteration.IndexOf("\\", 2);
            //        var fullPath = iteration.Insert(projectNameIndex, "\\Iteration");
            //        var nodeInfo = _smth.GetNodeFromPath(fullPath);
            //    }
            //}
            //var team = _teamService.QueryTeams(_project.Uri.ToString()).FirstOrDefault();

            //if (team != null)
            //{
            //    var configs = _teamConfiguration.GetTeamConfigurationsForUser(
            //        .Select(x => x.Uri.ToString())).ToList();

            //    if (configs.Any())
            //    {
            //        foreach (var config in configs)
            //        {
            //            var iterations = config
            //                .TeamSettings
            //                .IterationPaths
            //                .Select(x => )
            //        }
            //    }
            //}

            return 7;
        }

        public IList<WorkItem> GetMyWorkItems()
        {
            var quarry = $"select * from {Sql.Tables.WorkItems} " +
                         $"where {Sql.AssignedToMeCondition} " +
                         $"and {Sql.Fields.State} <> '{WorkItemStates.Closed}' " +
                         $"and {Sql.Fields.State} <> '{WorkItemStates.Removed}' " +
                         // Все, кроме Код ревью, они мусорные
                         $"and {Sql.Fields.WorkItemType} <> '{WorkItemTypes.CodeReview}'";

            var items = _itemStore.Query(quarry);

            return items.OfType<WorkItem>().ToList();
        }

        public WorkItem CreateTask(string title, WorkItem parent, uint hours)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            WorkItemType taskType;

            try
            {
                taskType = parent.Project.WorkItemTypes[WorkItemTypes.Task];
            }
            catch (Exception e)
            {
                // Выбранный тип не поддерживается проектом
                Trace.Write(e);
                throw new Exception(
                    $"Supported work item types:\n{string.Join("\n", parent.Project.WorkItemTypes.OfType<WorkItemType>().Select(x => x.Name))}");
            }

            var task = new WorkItem(taskType)
            {
                State = WorkItemStates.New,
                Title = title,
                AreaPath = parent.AreaPath,
                IterationPath = parent.IterationPath,
            };

            task.Fields[CoreField.AssignedTo].Value = _project.AuthorizedIdentity.DisplayName;
            task.Fields[WorkItems.Fields.Remaining].Value = hours;
            task.Fields[WorkItems.Fields.Planning].Value = hours;

            WorkItemLinkType link;

            try
            {
                link = _itemStore.WorkItemLinkTypes[WorkItems.LinkTypes.ParentLink];
            }
            catch (Exception e)
            {
                Trace.Write(e);
                throw new Exception(
                    $"Supported link types:\n{string.Join("\n", _itemStore.WorkItemLinkTypes.Select(x => x.ReferenceName))}");
            }

            // Сначала сохраняем как новый таск
            task.Save();

            Trace.WriteLine($"Created task {task.Id}");

            // Потом меняем статус в актив
            task.State = WorkItemStates.Active;
            task.Save();

            Trace.WriteLine($"Task status changed to {task.State}\n" +
                            "--------- BEFORE LINKING --------\n" +
                            $"Task links count: {task.Links.Count}, " +
                            $"Parent links count: {task.Links.Count}");

            // После сохранения привязываем
            parent.Links.Add(new RelatedLink(link.ReverseEnd, task.Id));
            parent.Save();

            Trace.WriteLine($"--------- AFTER LINKING --------\n" +
                            $"Task links count: {task.Links.Count}, " +
                            $"Parent links count: {task.Links.Count}");

            return task;
        }

        /// <exception cref="Exception"></exception>
        public int GetWriteOffHours(DateTime from, DateTime to)
        {
            var result = 0;

            if (from > to)
                throw new Exception($"{nameof(from)} should be earlier than {nameof(to)}");

            var querry = $"select * from {Sql.Tables.WorkItems} " +
                         $"where {Sql.Fields.WorkItemType} = '{WorkItemTypes.Task}' " +
                         $"and {Sql.WasEverChangedByMeCondition} " +
                         $"and {Sql.Fields.ChangedDate} >= '{from.ToShortDateString()}' " +
                         $"and {Sql.Fields.ChangedDate} <= '{to.ToShortDateString()}' ";

            var tasks = _itemStore.Query(querry);

            foreach (WorkItem task in tasks)
            {
                var revisions = task
                    .Revisions
                    .OfType<Revision>()
                    .Where(x => x.Fields[WorkItems.Fields.Complited]?.Value != null
                                && x.Fields[WorkItems.Fields.ChangedBy]?.Value != null
                                && x.Fields[CoreField.ChangedDate].Value is DateTime)
                    .ToList();

                double previouse = 0;

                foreach (var revision in revisions)
                {
                    // Был ли в этот момент таск на мне
                    var assignedToMe = revision.Fields[CoreField.AssignedTo]?.Value is string assigned
                                       && string.Equals(_itemStore.UserDisplayName, assigned);

                    // Был ли таск изменен мной
                    var changedByMe = revision.Fields[WorkItems.Fields.ChangedBy]?.Value is string owner
                                       && string.Equals(_itemStore.UserDisplayName, owner);

                    var correctTime = revision.Fields[CoreField.ChangedDate].Value is DateTime time
                                           && from <= time
                                           && time <= to;

                    var completed = (double) revision.Fields[WorkItems.Fields.Complited].Value;

                    // Списанное время
                    var delta = completed - previouse;

                    previouse = completed;

                    if (delta <= 0)
                        continue;

                    if (!correctTime)
                    {
                        continue;
                    }

                    if (!changedByMe)
                    {
                        Trace.WriteLine($"{revision.Fields[WorkItems.Fields.ChangedBy]?.Value} is changed completed work for you");
                        continue;
                    }

                    if (!assignedToMe)
                    {
                        Trace.WriteLine($"{revision.Fields[CoreField.AssignedTo]?.Value} took your task");
                        continue;
                    }

                    result += (int) delta;
                }
            }

            return result;
        }

        #endregion

        #region IItemTracker


        public void Start()
        {
            _paused = false;
            Subscribe();

            Trace.WriteLine("Starting observe work items changes");
        }

        public void Pause()
        {
            _paused = true;

            _listener.Pause();

            Trace.WriteLine("Pausing observe work items changes");
        }

        #endregion
    }
}
