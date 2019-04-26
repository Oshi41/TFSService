using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI
{
    public interface ITfs : IDisposable
    {
        /// <summary>
        /// Пользователь закоммитил изменения
        /// </summary>
        event EventHandler<CommitCheckinEventArgs> Checkin;

        /// <summary>
        /// На пользователя назначили новый рабочий элемент
        /// </summary>
        event EventHandler<WorkItem> NewItem;

        /// <summary>
        /// Списываю часы в указанный таск
        /// </summary>
        /// <param name="item">Таск, куда списываю</param>
        /// <param name="hours">Кол-во часов</param>
        void WriteHours(WorkItem item, byte hours);

        /// <summary>
        /// Возвращает список прилинкованных рабочих элементов к набору изменений
        /// </summary>
        /// <param name="changeset">ID набора изменений</param>
        /// <returns></returns>
        IList<WorkItem> GetAssotiatedItems(int changeset);

        /// <summary>
        /// Ищет рабочий эжлемент по номеру
        /// </summary>
        /// <param name="id">Номер рабочего элемента</param>
        /// <returns></returns>
        WorkItem FindById(int id);

        /// <summary>
        /// Возвращает кол-во часов, которое необходимо списать за день
        /// </summary>
        /// <returns></returns>
        int GetCapacity();
    }
}
