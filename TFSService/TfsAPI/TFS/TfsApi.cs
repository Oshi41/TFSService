using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace TfsAPI.TFS
{
    public class TfsApi : ITfsApi
    {
        #region Fields

        protected readonly TfsTeamProjectCollection _project;
        private readonly WorkItemStore _itemStore;
        private readonly ILinking _linking;

        /// <summary>
        /// Запрос на получение всех элементов на мне
        /// </summary>
        private readonly string _myItemsQuerry;

        #endregion

        /// <param name="url">Строка подключения к TFS</param>
        /// <param name="owner">От какого имени действуем</param>
        public TfsApi(string url, string owner = null)
        {
            _project = new TfsTeamProjectCollection(new Uri(url));

            Trace.WriteLine("Connected to " + _project.Name);

            _itemStore = _project.GetService<WorkItemStore>();
            _linking = _project.GetService<ILinking>();
            
            Name = owner ?? _itemStore.UserDisplayName;

            Trace.WriteLine($"{nameof(TfsApi)}.ctor: Acting from {Name}");

            _myItemsQuerry = $"select * from {Sql.Tables.WorkItems} " +
                                                 $"where {Sql.Fields.AssignedTo} = '{Name}' " +
                                                 $"and {Sql.Fields.State} <> '{WorkItemStates.Closed}' " +
                                                 $"and {Sql.Fields.State} <> '{WorkItemStates.Removed}' ";
        }

        #region ITfsApi

        public Revision WriteHours(WorkItem item, byte hours, bool setActive)
        {
            item.AddHours(hours, setActive);
            SaveElemnt(item);
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
                ToolSpecificId = changeset.ToString(),
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
            // TODO scan inet for answer!!!

            return 7;
        }

        public IList<WorkItem> GetMyWorkItems()
        {
            var items = _itemStore.Query(_myItemsQuerry);

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
            SaveElemnt(task);

            Trace.WriteLine($"Created task {task.Id}");

            // Потом меняем статус в актив
            task.State = WorkItemStates.Active;
            SaveElemnt(task);

            Trace.WriteLine($"Task status changed to {task.State}\n" +
                            "--------- BEFORE LINKING --------\n" +
                            $"Task links count: {task.Links.Count}, " +
                            $"Parent links count: {task.Links.Count}");

            // После сохранения привязываем
            parent.Links.Add(new RelatedLink(link.ReverseEnd, task.Id));
            SaveElemnt(task);

            Trace.WriteLine($"--------- AFTER LINKING --------\n" +
                            $"Task links count: {task.Links.Count}, " +
                            $"Parent links count: {task.Links.Count}");

            return task;
        }

        public List<KeyValuePair<Revision, int>> GetCheckins(DateTime from, DateTime to)
        {
            var result = new List<KeyValuePair<Revision, int>>();

            // Рабочие элементы в TFS находятся по дате
            // Т.к. TFS некорректно отрабатывает с ">=",
            // работаем с ">". Для этого нужно исключить переданный день
            from = from.AddDays(-1).Date;
            to = to.AddDays(1).Date;

            if (from >= to)
                throw new Exception($"{nameof(from)} should be earlier than {nameof(to)}");

            var querry = $"select * from {Sql.Tables.WorkItems} " +
                         $"where {Sql.Fields.WorkItemType} = '{WorkItemTypes.Task}' " +
                         $"and ever [Changed By] = '{Name}' " +
                         $"and {Sql.Fields.ChangedDate} > '{from.ToShortDateString().Replace(".", "/")}' " +
                         $"and {Sql.Fields.ChangedDate} < '{to.ToShortDateString().Replace(".", "/")}' ";

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

            foreach (var request in requests)
            {
                var responses = request
                    .WorkItemLinks
                    .OfType<WorkItem>()
                    .Where(x => x.IsTypeOf(WorkItemTypes.ReviewResponse))
                    .ToList();

                Trace.WriteLine($"Request {request.Id} has {responses.Count} responses");

                if (canClose(request, responses))
                {
                    result.Add(request);

                    request.Close();
                    SaveElemnt(request);
                }
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
                    if (parent != null)
                    {
                        result.Add(parent);
                    }
                }
            }

            return result;
        }

        public string Name { get; }

        #endregion

        private void SaveElemnt(WorkItem item)
        {
#if DEBUG
            Trace.WriteLine($"Представь, что мы сохранили {item.Id}");

#else
            if (_itemStore.UserDisplayName != _userName)
            {
                Trace.WriteLine($"Can't check-in from {_userName}, authorized as {_itemStore.UserDisplayName}");
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
    }
}
