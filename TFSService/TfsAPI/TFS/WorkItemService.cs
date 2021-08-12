using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;
using TfsAPI.Logger;
using TfsAPI.Rules;
using TfsAPI.TFS.Trend;

namespace TfsAPI.TFS
{
    public class WorkItemService : IWorkItem
    {
        /// <summary>
        /// Сервис для работы с рабочими элементами
        /// </summary>
        /// <param name="connect">Подключение к TFS</param>
        /// <param name="owner">От сьего лица действуем</param>
        public WorkItemService(IConnect connect, string owner)
        {
            Connect = connect;
            Name = owner ?? Connect.WorkItemStore.UserDisplayName;

            LoggerHelper.WriteLine($"Acting from {Name}");

            _myItemsQuerry = new WiqlBuilder()
                .AssignedTo()
                .WithStates("and", "<>", "and", WorkItemStates.Closed, WorkItemStates.Removed)
                .ToString();
        }

        public void SaveElement(WorkItem item)
        {
            if (item == null)
            {
                LoggerHelper.WriteLine($"item is null");
                return;
            }

#if DEBUG
            LoggerHelper.WriteLine($"Imagine that we have saved {item.Id} item");

            try
            {
                item.Save();
                item.SyncToLatest();
            }
            catch (Exception e)
            {
                LoggerHelper.WriteLine(e);
            }

#else
            if (_connect.WorkItemStore.UserDisplayName != Name)
                LoggerHelper.WriteLine(
                    $"Can't check-in from {Name}, authorized as {_connect.WorkItemStore.UserDisplayName}");
            else
                try
                {
                    item.Save();
                    item.SyncToLatest();
                }
                catch (Exception e)
                {
                    LoggerHelper.WriteLine(e);
                }
#endif
        }

        public string MyItemsQuarry => _myItemsQuerry;

        public static async Task<bool> CheckConnection(string url)
        {
            bool CheckConnectSync()
            {
                try
                {
                    using (var proj = new TfsTeamProjectCollection(new Uri(url)))
                    {
                        return proj.AuthorizedIdentity.DisplayName != null;
                    }
                }
                catch (Exception e)
                {
                    LoggerHelper.WriteLine(e);
                    return false;
                }
            }

            return await Task.Run(CheckConnectSync);
        }

        #region Fields

        protected readonly IConnect Connect;

        /// <summary>
        ///     Запрос на получение всех элементов на мне
        /// </summary>
        private readonly string _myItemsQuerry;

        #endregion

        #region ITfsApi

        public IList<WorkItem> GetAssociateItems(int changeset)
        {
            var result = new List<WorkItem>();

            // Создал объект для получения URL
            var setId = new ArtifactId
            {
                Tool = ToolNames.VersionControl,
                ArtifactType = WorkItemTypes.ChangeSet,
                ToolSpecificId = changeset.ToString()
            };

            // По этому URL буду искать линкованные элементы
            var uri = LinkingUtilities.EncodeUri(setId);

            // Нашел связи
            var linked = Connect.Linking.GetReferencingArtifacts(new[] {uri});

            LoggerHelper.WriteLine($"Founded {linked.Length} links");

            foreach (var artifact in linked)
            {
                // Распарсил url
                var artifactId = LinkingUtilities.DecodeUri(artifact.Uri);
                // Какая-то хитрая проверка
                if (string.Equals(artifactId.Tool, ToolNames.WorkItemTracking, StringComparison.OrdinalIgnoreCase))
                {
                    // Нашёл элемент
                    var item = Connect.WorkItemStore.GetWorkItem(Convert.ToInt32(artifactId.ToolSpecificId));
                    // Добавил
                    result.Add(item);
                }
            }

            LoggerHelper.WriteLineIf(result.Any(),
                $" Changeset {changeset} linked with items: {string.Join(", ", result.Select(x => x.Id))}");

            return result;
        }

        public WorkItem FindById(int id)
        {
            try
            {
                return Connect.WorkItemStore.GetWorkItem(id);
            }
            catch (Exception e)
            {
                LoggerHelper.WriteLine(e);
                return null;
            }
        }

        public IList<WorkItem> Search(string text, params string[] allowedTypes)
        {
            var quarry = new WiqlBuilder()
                .ContainsInFields("where",
                    text,
                    Sql.Fields.History,
                    Sql.Fields.Title,
                    Sql.Fields.Description);

            // Ищу только указанные типы
            if (!allowedTypes.IsNullOrEmpty()) quarry.WithItemTypes("and", "=", allowedTypes);

            var items = Connect.WorkItemStore.Query(quarry.ToString());

            LoggerHelper.WriteLine($"Founded {items.Count} items");

            return items.OfType<WorkItem>().ToList();
        }

        public virtual WorkItemCollection GetMyWorkItems()
        {
            return QueryItems(_myItemsQuerry);
        }

        public WorkItemCollection QueryItems(string query)
        {
            return Connect.WorkItemStore.Query(query);
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
                LoggerHelper.WriteLine(e);
                throw new Exception(
                    $"Supported work item types:\n{string.Join("\n", parent.Project.WorkItemTypes.OfType<WorkItemType>().Select(x => x.Name))}");
            }

            var task = new WorkItem(taskType)
            {
                State = WorkItemStates.New,
                Title = title,
                AreaPath = parent.AreaPath,
                IterationPath = parent.IterationPath
            };

            task.Fields[CoreField.AssignedTo].Value = Connect.Tfs.AuthorizedIdentity.DisplayName;
            task.Fields[WorkItems.Fields.Remaining].Value = hours;
            task.Fields[WorkItems.Fields.Planning].Value = hours;
            task.Fields[WorkItems.Fields.Complited].Value = 0;

            WorkItemLinkType link;

            try
            {
                link = Connect.WorkItemStore.WorkItemLinkTypes[WorkItems.LinkTypes.ParentLink];
            }
            catch (Exception e)
            {
                LoggerHelper.WriteLine(e);
                throw new Exception(
                    $"Supported link types:\n{string.Join("\n", Connect.WorkItemStore.WorkItemLinkTypes.Select(x => x.ReferenceName))}");
            }

            // Сначала сохраняем как новый таск
            SaveElement(task);

            LoggerHelper.WriteLine($"Created task {task.Id}");

            // Потом меняем статус в актив
            task.State = WorkItemStates.Active;
            SaveElement(task);

            LoggerHelper.WriteLine($"Task status changed to {task.State}\n" +
                                   "--------- BEFORE LINKING --------\n" +
                                   $"Task links count: {task.Links.Count}, " +
                                   $"Parent links count: {task.Links.Count}");

            // После сохранения привязываем
            task.Links.Add(new RelatedLink(link.ReverseEnd, parent.Id));
            SaveElement(task);

            LoggerHelper.WriteLine($"--------- AFTER LINKING --------\n" +
                                   $"Task links count: {task.Links.Count}, " +
                                   $"Parent links count: {task.Links.Count}");

            return task;
        }

        public bool IsAssignedToMe(WorkItem item)
        {
            if (item == null)
                return false;

            return item.Fields[CoreField.AssignedTo]?.Value is string owner
                   && string.Equals(owner, Name);
        }

        public List<WorkItem> GetParents(params WorkItem[] items)
        {
            var result = new List<WorkItem>();

            foreach (var item in items.Where(x => x != null
                                                  && x.WorkItemLinks.Count > 0))
            {
                var parentLinks = item
                    .WorkItemLinks
                    .OfType<WorkItemLink>()
                    .Where(x => x.LinkTypeEnd.Name == "Parent")
                    .ToList();

                foreach (var link in parentLinks)
                {
                    var parent = Connect.WorkItemStore.GetWorkItem(link.TargetId);
                    if (parent != null) result.Add(parent);
                }
            }

            return result;
        }

        public Dictionary<int, WorkItem> FindById(IEnumerable<int> ids)
        {
            if (ids.IsNullOrEmpty())
                return new Dictionary<int, WorkItem>();

            var builder = new WiqlBuilder();

            foreach (var id in ids.Distinct()) builder = builder.WithNumber("or", id);

            var items = QueryItems(builder.ToString());

            return items.ToDictionary(x => x.Id);
        }

        public string Name { get; }

        #endregion
    }
}