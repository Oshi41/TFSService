using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Extentions
{
    public static class WorkItemExtensions
    {
        /// <summary>
        /// Является ли данный рабочий элемент активным
        /// </summary>
        /// <param name="item">Рабочий элемент</param>
        /// <returns></returns>
        public static bool IsActive(this WorkItem item)
        {
            return string.Equals(item?.State, WorkItemStates.Active);
        }

        /// <summary>
        /// Является ли данный рабочий элемент таском
        /// </summary>
        /// <param name="item">Рабочий элемент</param>
        /// <returns></returns>
        public static bool IsTask(this WorkItem item)
        {
            return string.Equals(item?.Type?.Name, WorkItemTypes.Task);
        }

        /// <summary>
        /// Списывает часы у задания. Не сохраняет изменения!
        /// </summary>
        /// <param name="item">рабочий элемента</param>
        /// <param name="hours">Кол-во часов для списывания</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static void AddHours(this WorkItem item, byte hours)
        {
            if (item == null)
                throw new ArgumentException(nameof(item));

            if (hours == 0)
                throw new ArgumentException(nameof(hours));

            if (!item.IsTask() || !item.IsActive())
                throw new Exception("item must be an active task");

            var total = int.Parse(item[WorkItems.Fields.Complited].ToString()) + hours;
            var remain = Math.Max(0, int.Parse(item[WorkItems.Fields.Remaining].ToString()) - hours);

            item[WorkItems.Fields.Complited] = total;
            item[WorkItems.Fields.Remaining] = remain;
        }
    }
}
