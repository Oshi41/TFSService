using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;
using TfsAPI.Logger;
using TfsAPI.Rules;
using TfsAPI.TFS;
using TfsAPI.TFS.Trend;

namespace TfsAPI.TFS
{
    public class WriteOffService : IWriteOff
    {
        private readonly IWorkItem _workItemService;
        private readonly IConnect _connect;

        public WriteOffService(IConnect connect, IWorkItem workItemService)
        {
            _connect = connect;
            _workItemService = workItemService;
        }

        #region ITfsApi

        public Revision WriteHours(WorkItem item, byte hours, bool setActive)
        {
            item.SyncToLatest();

            item.AddHours(hours, setActive);
            _workItemService.SaveElement(item);
            LoggerHelper.WriteLine($"From task {item.Id} was writed off {hours} hour(s)");

            var revisions = item.Revisions.OfType<Revision>();
            var finded = revisions
                .Where(x => x.Fields.TryGetById((int)CoreField.ChangedBy) != null
                            && x.Fields.Contains(WorkItems.Fields.Complited)
                            && Equals(_connect.Name, x.Fields[CoreField.ChangedBy].Value)
                            && x.Fields[WorkItems.Fields.Complited].Value != null)
                .OrderByDescending(x => (DateTime)x.Fields[CoreField.ChangedDate].Value)
                .ToList();

            return finded.FirstOrDefault();
        }

        public int GetCapacity()
        {
            var today = DateTime.Today;
            var answer = GetCapacity(today, today);
            var result = answer.Sum(x => x.GetCapacity(_connect.Name));

            return result;
        }

        public virtual List<TeamCapacity> GetCapacity(DateTime start, DateTime end)
        {
            var searcher = new CapacitySearcher(_connect);
            return searcher.SearchCapacities(_connect.Name, start, end);
        }

        public List<KeyValuePair<Revision, int>> GetWriteoffs(DateTime from, DateTime to)
        {
            var result = new List<KeyValuePair<Revision, int>>();

            // Рабочие элементы в TFS находятся по дате
            // Т.к. TFS некорректно отрабатывает с ">=",
            // работаем с ">". Для этого нужно исключить переданный день
            @from = @from.AddDays(-1).Date;
            to = to.AddDays(1).Date;

            if (@from >= to)
                throw new Exception($"{nameof(@from)} should be earlier than {nameof(to)}");

            var query = new WiqlBuilder()
                .AssignedTo()
                .WithItemTypes("and", "=", WorkItemTypes.Task)
                .EverChangedBy("and")
                .ChangedDate("and", @from, ">")
                .ChangedDate("and", to, "<");

            var tasks = _connect.WorkItemStore.Query(query.ToString());

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
                                       && string.Equals(_connect.Name, assigned);

                    // Был ли таск изменен мной
                    var changedByMe = revision.Fields[WorkItems.Fields.ChangedBy]?.Value is string owner
                                      && string.Equals(_connect.Name, owner);

                    var correctTime = revision.Fields[CoreField.ChangedDate].Value is DateTime time
                                      && @from < time.Date
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
                        LoggerHelper.WriteLine(
                            $"{revision.Fields[WorkItems.Fields.ChangedBy]?.Value} is changed completed work for you");
                        continue;
                    }

                    if (!assignedToMe)
                    {
                        LoggerHelper.WriteLine($"{revision.Fields[CoreField.AssignedTo]?.Value} took your task");
                        continue;
                    }

                    result.Add(new KeyValuePair<Revision, int>(revision, delta));
                }
            }

            return result;
        }

        public Chart GetForMonth(DateTime from, DateTime to, int dailyCapacity)
        {
            // Обозначил граница месяца
            //var from = new DateTime(time.Year, time.Month, 1);
            //var to = from.AddMonths(1).AddDays(-1);

            // Ограничиваем дату
            if (to > DateTime.Now)
            {
                to = DateTime.Now;
            }

            var dates = new List<DateTime>();

            // заполнил даты
            {
                var current = @from;
                while (current <= to)
                {
                    if (!current.IsHoliday())
                        dates.Add(current);

                    current = current.AddDays(1);
                }
            }

            // Запрос на получение тасков, в которых я когда либо участвовал
            // чтобы не запрашивать все таски, указываю пределы времени:
            // Когда таск был создан и когда таск был закрыт
            var builder = new WiqlBuilder()
                .EverChangedBy("from")
                .WithItemTypes("and", "=", WorkItemTypes.Task)
                .EverChangedBy("and")
                .ChangedDate("and", @from, ">=")
                .CreatedDate("and", to, "<=");

            var items = _workItemService.QueryItems(builder.ToString());

            var taskRevisions = items
                .OfType<WorkItem>()
                .SelectMany(x => x.Revisions.OfType<Revision>())
                // Нашли которые мы меняли
                .Where(x => x.ChangedBy(_connect.Name) == true)
                // сгруппировал по рабочему элоементу
                .GroupBy(x => x.WorkItem)
                .ToDictionary(x => x.Key, x => x.OrderBy(r => r.Index).ToList());

            var chart = new Chart();

            foreach (var date in dates)
            {
                var point = new Point
                {
                    Time = date
                };

                foreach (var pair in taskRevisions)
                {
                    var remainings = pair
                        .Value
                        // Нашёл изменения за этот день
                        .Where(x => date.SameDay(x.ChangedDate()))
                        .Select(x => x.RemainingWork())
                        // только актуальные значения
                        .Where(x => x >= 0);

                    point.Value += remainings.LastOrDefault();
                }

                chart.Items.Add(point);
            }

            double writeOff = 0;

            // Идём с конца, так проще
            foreach (var date in dates.OrderByDescending(x => x))
            {
                // каждый раз добавляем в начало графика
                chart.Available.Insert(0, new Point
                {
                    Time = date,
                    Value = writeOff += dailyCapacity
                });
            }

            // Сколько списывал в этом месяце
            var checkins = GetWriteoffs(@from, to);
            // всего часов работы
            var total = checkins.Sum(x => x.Value);

            foreach (var date in dates)
            {
                var forToday = checkins.Where(x => date.SameDay(x.Key.ChangedDate())).Sum(x => x.Value);

                chart.WriteOff.Add(new Point
                {
                    Time = date,
                    Value = total -= forToday
                });
            }

            return chart;
        }

        public string Name => _connect.Name;

        #endregion
    }
}