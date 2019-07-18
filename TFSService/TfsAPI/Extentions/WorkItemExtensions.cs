using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;

namespace TfsAPI.Extentions
{
    public static class WorkItemExtensions
    {
        /// <summary>
        ///     Сверяет переданное состояние рабочего элемента с фактическим.
        /// </summary>
        /// <param name="item">Рабочий элемент</param>
        /// <param name="type">Состояние. См <see cref="WorkItemStates" /></param>
        /// <returns></returns>
        public static bool HasState(this WorkItem item, string type)
        {
            return string.Equals(item?.State, type);
        }

        /// <summary>
        ///     Сверяет переданные типы рабочего элемента с фактическим
        /// </summary>
        /// <param name="item">Рабочий элемент</param>
        /// <param name="type">Типы рабочего элемента. См. <see cref="WorkItemTypes" /></param>
        /// <returns></returns>
        public static bool IsTypeOf(this WorkItem item, params string[] types)
        {
            if (types.IsNullOrEmpty())
                return false;

            var closedReason = item?.Type?.Name;
            return types.Contains(closedReason);
        }

        /// <summary>
        ///     Сверяет причину закрытия рабочего элемента
        /// </summary>
        /// <param name="item">Рабочий элемент</param>
        /// <param name="type">Список причин закрытия. См. <see cref="WorkItems.ClosedStatus" /></param>
        /// <returns></returns>
        public static bool HasClosedReason(this WorkItem item, params string[] types)
        {
            if (types.IsNullOrEmpty())
                return false;

            var closedReason = item?.Fields[WorkItems.Fields.ClosedStatus]?.Value?.ToString();
            return types.Contains(closedReason);
        }

        /// <summary>
        ///     Доступен ли данный таск для списания времени
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsTaskAvailable(this WorkItem item)
        {
            return item.IsTypeOf(WorkItemTypes.Task)
                   && (
                       item.HasState(WorkItemStates.Active)
                       || item.HasState(WorkItemStates.New)
                   );
        }

        /// <summary>
        ///     Списывает часы у задания. Не сохраняет изменения!
        /// </summary>
        /// <param name="item">рабочий элемента</param>
        /// <param name="hours">Кол-во часов для списывания</param>
        /// <param name="setActive">Нужно ли выставить Active State для таска</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static void AddHours(this WorkItem item, byte hours, bool setActive = false)
        {
            Trace.WriteLine($"{nameof(WorkItemExtensions)}.{nameof(AddHours)}: adding {hours} to Items {item.Id}");

            if (item == null)
                throw new ArgumentException(nameof(item));

            if (hours == 0)
                throw new ArgumentException(nameof(hours));

            if (!item.IsTypeOf(WorkItemTypes.Task))
                throw new Exception("item must be have a Task type");

            if (!item.HasState(WorkItemStates.Active))
            {
                if (!setActive) throw new Exception("Task should have an active state");

                item.State = "Active";
            }

            if (!double.TryParse(item[WorkItems.Fields.Complited]?.ToString(), out var completed))
            {
                Trace.WriteLine($"Can't parse item [{WorkItems.Fields.Complited}] field");
                completed = 0;
            }

            if (!double.TryParse(item[WorkItems.Fields.Remaining]?.ToString(), out var remaining))
            {
                Trace.WriteLine($"Can't parse item [{WorkItems.Fields.Remaining}] field");
                remaining = 0;
            }

            completed += hours;
            remaining = Math.Max(0, remaining - hours);


            if (!item.IsOpen)
            {
                // Открываем элемент для редактирования
                item.Open();
                Trace.WriteLine($"{nameof(WorkItemExtensions)}.{nameof(AddHours)}: Opened work item");
            }

            item.Fields[WorkItems.Fields.Complited].Value = completed;
            item.Fields[WorkItems.Fields.Remaining].Value = remaining;

            Trace.WriteLine(
                $"{nameof(WorkItemExtensions)}.{nameof(AddHours)}: Added hours");
        }

        /// <summary>
        ///     Работа по элементу не прекращена
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsNotClosed(this WorkItem item)
        {
            return !item.HasState(WorkItemStates.Closed);
        }


        /// <summary>
        ///     Важные рабочие элемента с высочайшим приоритетом
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsRare(this WorkItem item)
        {
            return item.IsTypeOf(WorkItemTypes.Feature, WorkItemTypes.Incident);
        }

        /// <summary>
        ///     Стандартные рабочие элементы
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsWorkItem(this WorkItem item)
        {
            return item.IsTypeOf(WorkItemTypes.Pbi, WorkItemTypes.Improvement);
        }

        /// <summary>
        ///     Рабочий элемент - баг
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsBug(this WorkItem item)
        {
            return item.IsTypeOf(WorkItemTypes.Bug);
        }

        /// <summary>
        ///     Открывает элементв на web интерфейсе
        /// </summary>
        /// <param name="item"></param>
        public static void OpenLink(this WorkItem item)
        {
            // TODO поискать ответ
        }
    }
}