using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;

namespace TfsAPI.Extentions
{
    public static class WorkItemExtensions
    {
        /// <summary>
        /// Сверяет переданное состояние рабочего элемента с фактическим.
        /// </summary>
        /// <param name="item">Рабочий элемент</param>
        /// <param name="type">Состояние. См <see cref="WorkItemStates"/></param>
        /// <returns></returns>
        public static bool HasState(this WorkItem item, string type)
        {
            return string.Equals(item?.State, type);
        }

        /// <summary>
        /// Сверяет переданный тип рабочего элемента с фактическим
        /// </summary>
        /// <param name="item">Рабочий элемент</param>
        /// <param name="type">Тип рабочего элемента. См. <see cref="WorkItemTypes"/></param>
        /// <returns></returns>
        public static bool IsTypeOf(this WorkItem item, string type)
        {
            return string.Equals(item?.Type?.Name, type);
        }

        /// <summary>
        /// Сверяет причину закрытия рабочего элемента
        /// </summary>
        /// <param name="item">Рабочий элемент</param>
        /// <param name="type">Список причин закрытия. См. <see cref="WorkItems.ClosedStatus"/></param>
        /// <returns></returns>
        public static bool HasClosedReason(this WorkItem item, params string[] types)
        {
            if (types.IsNullOrEmpty())
                return false;

            var closedReason = item?.Fields[WorkItems.Fields.ClosedStatus]?.Value?.ToString();
            return types.Contains(closedReason);
        }

        /// <summary>
        /// Доступен ли данный таск для списания времени
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
        /// Списывает часы у задания. Не сохраняет изменения!
        /// </summary>
        /// <param name="item">рабочий элемента</param>
        /// <param name="hours">Кол-во часов для списывания</param>
        /// <param name="setActive">Нужно ли выставить Active State для таска</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static void AddHours(this WorkItem item, byte hours, bool setActive = false)
        {
            if (item == null)
                throw new ArgumentException(nameof(item));

            if (hours == 0)
                throw new ArgumentException(nameof(hours));

            if (!item.IsTypeOf(WorkItemTypes.Task))
                throw new Exception("item must be have a Task type");

            if (!item.HasState(WorkItemStates.Active))
            {
                if (!setActive)
                {
                    throw new Exception("Task should have an active state");
                }

                item.State = "Active";
            }

            var total = int.Parse(item[WorkItems.Fields.Complited].ToString()) + hours;
            var remain = Math.Max(0, int.Parse(item[WorkItems.Fields.Remaining].ToString()) - hours);

            item[WorkItems.Fields.Complited] = total;
            item[WorkItems.Fields.Remaining] = remain;
        }
    }
}
