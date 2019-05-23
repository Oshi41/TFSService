using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;

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
        /// Доступен ли данный таск для списания времени
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsTaskAvailable(this WorkItem item)
        {
            return item.IsTask()
                   && (
                       item.IsActive()
                       || string.Equals(item?.State, WorkItemStates.New)
                   );
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
        /// Является ли данный элемент запросом кода на проверку
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsReviewRequest(this WorkItem item)
        {
            return string.Equals(item?.Type?.Name, WorkItemTypes.ReviewRequest);
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

            if (!item.IsTask())
                throw new Exception("item must be have a Task type");

            if (!item.IsActive())
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
