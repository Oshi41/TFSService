﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;
using TfsAPI.Rules;

namespace TfsAPI.TFS
{
    public class TfsApi : ITfsApi
    {
        /// <param name="url">Строка подключения к TFS</param>
        /// <param name="owner">От какого имени действуем</param>
        public TfsApi(string url, string owner = null)
        {
            _project = new TfsTeamProjectCollection(new Uri(url));

            Trace.WriteLine("Connected to " + _project.Name);

            _itemStore = _project.GetService<WorkItemStore>();
            _linking = _project.GetService<ILinking>();
            _versionControl = _project.GetService<VersionControlServer>();

            Name = owner ?? _itemStore.UserDisplayName;

            Trace.WriteLine($"{nameof(TfsApi)}.ctor: Acting from {Name}");

            _myItemsQuerry = new WiqlBuilder()
                .AssignedTo()
                .WithStates("and", "<>", "and", WorkItemStates.Closed, WorkItemStates.Removed)
                .ToString();
        }

        private void SaveElement(WorkItem item)
        {
#if DEBUG
            Trace.WriteLine($"Imagine that we have saved {item.Id} item");

#else
            if (_itemStore.UserDisplayName != Name)
            {
                Trace.WriteLine($"Can't check-in from {Name}, authorized as {_itemStore.UserDisplayName}");
            }
            else
            {
                item?.Save();
            }
#endif
        }

        public static async Task<bool> CheckConnection(string url)
        {
            bool CheckConnectSync()
            {
                try
                {
                    var proj = new TfsTeamProjectCollection(new Uri(url));
                    return true;
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                    return false;
                }
            }

            return await Task.Run((Func<bool>)CheckConnectSync);
        }

        #region Fields

        protected readonly TfsTeamProjectCollection _project;
        private readonly WorkItemStore _itemStore;
        private readonly ILinking _linking;
        private readonly VersionControlServer _versionControl;

        /// <summary>
        ///     Запрос на получение всех элементов на мне
        /// </summary>
        private readonly string _myItemsQuerry;

        #endregion

        #region ITfsApi

        public Revision WriteHours(WorkItem item, byte hours, bool setActive)
        {
            item.AddHours(hours, setActive);
            SaveElement(item);
            Trace.WriteLine($"From task {item.Id} was writed off {hours} hour(s)");

            // TODO продебажить корректную ревизию

            return item
                .Revisions
                .OfType<Revision>()
                .Where(x => Equals(Name, x.Fields[CoreField.ChangedBy].Value)
                            && x.Fields[WorkItems.Fields.Complited].Value != null)
                .OrderByDescending(x => x.Fields[CoreField.ChangedDate])
                .FirstOrDefault();
        }

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
            var linked = _linking.GetReferencingArtifacts(new[] { uri });

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

            Trace.WriteLineIf(result.Any(),
                $"Changeset {changeset} linked with items: {string.Join(", ", result.Select(x => x.Id))}");

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
            var quarry = new WiqlBuilder()
                .ContainsInFields("where", 
                text, 
                Sql.Fields.History, 
                Sql.Fields.Title, 
                Sql.Fields.Description);

            // Ищу только указанные типы
            if (!allowedTypes.IsNullOrEmpty())
            {
                quarry.WithItemTypes("and", "=", allowedTypes);
            }

            var items = _itemStore.Query(quarry.ToString());

            Trace.WriteLine($"Tfs.Search: Founded {items.Count} items");

            return items.OfType<WorkItem>().ToList();
        }

        public int GetCapacity()
        {
            // TODO scan inet for answer!!!

            return 7;
        }

        public virtual IList<WorkItem> GetMyWorkItems()
        {
            return QueryItems(_myItemsQuerry);
        }

        public IList<WorkItem> QueryItems(string additionalQuery)
        {
            if (string.IsNullOrWhiteSpace(additionalQuery))
                return new List<WorkItem>();

            var items = _itemStore.Query(additionalQuery);

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
                IterationPath = parent.IterationPath
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
            SaveElement(task);

            Trace.WriteLine($"Created task {task.Id}");

            // Потом меняем статус в актив
            task.State = WorkItemStates.Active;
            SaveElement(task);

            Trace.WriteLine($"Task status changed to {task.State}\n" +
                            "--------- BEFORE LINKING --------\n" +
                            $"Task links count: {task.Links.Count}, " +
                            $"Parent links count: {task.Links.Count}");

            // После сохранения привязываем
            parent.Links.Add(new RelatedLink(link.ReverseEnd, task.Id));
            SaveElement(task);

            Trace.WriteLine("--------- AFTER LINKING --------\n" +
                            $"Task links count: {task.Links.Count}, " +
                            $"Parent links count: {task.Links.Count}");

            return task;
        }

        public List<KeyValuePair<Revision, int>> GetWriteoffs(DateTime from, DateTime to)
        {
            var result = new List<KeyValuePair<Revision, int>>();            

            // Рабочие элементы в TFS находятся по дате
            // Т.к. TFS некорректно отрабатывает с ">=",
            // работаем с ">". Для этого нужно исключить переданный день
            from = from.AddDays(-1).Date;
            to = to.AddDays(1).Date;

            if (from >= to)
                throw new Exception($"{nameof(from)} should be earlier than {nameof(to)}");            

            var query = new WiqlBuilder()
                .AssignedTo()
                .WithItemTypes("and", "=", WorkItemTypes.Task)
                .EverChangedBy("and")
                .ChangedDate("and", from, ">")
                .ChangedDate("and", to, "<");

            var tasks = _itemStore.Query(query.ToString());

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
                                       && string.Equals(Name, assigned);

                    // Был ли таск изменен мной
                    var changedByMe = revision.Fields[WorkItems.Fields.ChangedBy]?.Value is string owner
                                      && string.Equals(Name, owner);

                    var correctTime = revision.Fields[CoreField.ChangedDate].Value is DateTime time
                                      && from < time.Date
                                      && time.Date < to;

                    var completed = (double)revision.Fields[WorkItems.Fields.Complited].Value;

                    // Списанное время
                    var delta = (int)(completed - previouse);

                    previouse = completed;

                    if (delta < 1)
                        continue;

                    if (!correctTime) continue;

                    if (!changedByMe)
                    {
                        Trace.WriteLine(
                            $"{revision.Fields[WorkItems.Fields.ChangedBy]?.Value} is changed completed work for you");
                        continue;
                    }

                    if (!assignedToMe)
                    {
                        Trace.WriteLine($"{revision.Fields[CoreField.AssignedTo]?.Value} took your task");
                        continue;
                    }

                    result.Add(new KeyValuePair<Revision, int>(revision, delta));
                }
            }

            return result;
        }

        public bool IsAssignedToMe(WorkItem item)
        {
            if (item == null)
                return false;

            return item.Fields[CoreField.AssignedTo]?.Value is string owner
                   && string.Equals(owner, Name);
        }

        public List<WorkItem> CloseCompletedReviews(CanCloseReview canClose)
        {
            if (canClose == null)
                throw new ArgumentException(nameof(canClose));

            var quarry = _myItemsQuerry +
                         // Ищем только Code Review Request
                         $"and {Sql.Fields.WorkItemType} = '{WorkItemTypes.CodeReview}'";

            var requests = _itemStore.Query(quarry).OfType<WorkItem>().ToList();

            Trace.WriteLineIf(requests.Any(), $"Founded {requests.Count} requests");

            var result = new List<WorkItem>();

            var all = FindById(requests.SelectMany(x => x
                      .WorkItemLinks
                      .OfType<WorkItemLink>()
                      .Select(y => y.TargetId)));

            foreach (var request in requests)
            {
                var parents = request
                    .WorkItemLinks
                    .OfType<WorkItemLink>()
                    .Select(x => x.TargetId)
                    .ToList();

                var responses = all.Where(x => parents.Contains(x.Key)
                    && x.Value.IsTypeOf(WorkItemTypes.ReviewResponse))
                    .Select(x => x.Value)
                    .ToList();

                Trace.WriteLine($"Request {request.Id} has {responses.Count} responses");

                if (canClose(request, responses))
                {
                    result.Add(request);
                }
            }

            foreach (var item in result)
            {
                item.State = WorkItemStates.Closed;
                SaveElement(item);
            }

            Trace.WriteLineIf(result.Any(), $"Closed {result.Count} requests");

            return result;
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
                    var parent = _itemStore.GetWorkItem(link.TargetId);
                    if (parent != null) result.Add(parent);
                }
            }

            return result;
        }

        public List<Changeset> GetCheckins(DateTime from, DateTime to, string user = null)
        {
            var innerResults = _versionControl.QueryHistory(@"$\",
                VersionSpec.Latest,
                0,
                RecursionType.Full,
                user,
                new DateVersionSpec(from),
                new DateVersionSpec(to),
                int.MaxValue,
                false,
                false);

            return innerResults.OfType<Changeset>().ToList();
        }

        public Dictionary<int, WorkItem> FindById(IEnumerable<int> ids)
        {
            if (ids.IsNullOrEmpty())
                return new Dictionary<int, WorkItem>();

            var builder = new WiqlBuilder();

            foreach (var id in ids.Distinct())
            {
                builder = builder.WithNumber("or", id);
            }

            var items = QueryItems(builder.ToString());

            return items.ToDictionary(x => x.Id);
        }

        public string Name { get; }

        #endregion
    }
}