using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Interfaces
{
    public interface ITfs : IItemTracker, IDisposable
    {
        /// <summary>
        /// Пользователь закоммитил изменения
        /// </summary>
        event EventHandler<CommitCheckinEventArgs> Checkin;

        /// <summary>
        /// Списываю часы в указанный таск
        /// </summary>
        /// <param name="item">Таск, куда списываю</param>
        /// <param name="hours">Кол-во часов</param>
        /// <param name="setActive">Нужно ли выставить состояние таска в Active</param>
        void WriteHours(WorkItem item, byte hours, bool setActive);

        /// <summary>
        /// Возвращает список прилинкованных рабочих элементов к набору изменений
        /// </summary>
        /// <param name="changeset">ID набора изменений</param>
        /// <returns></returns>
        IList<WorkItem> GetAssociateItems(int changeset);

        /// <summary>
        /// Ищет рабочий эжлемент по номеру
        /// </summary>
        /// <param name="id">Номер рабочего элемента</param>
        /// <returns></returns>
        WorkItem FindById(int id);

        /// <summary>
        /// Производит поиск в названии, описании
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        IList<WorkItem> Search(string text);

        /// <summary>
        /// Возвращает кол-во часов, которое необходимо списать за день
        /// </summary>
        /// <returns></returns>
        int GetCapacity();

        /// <summary>
        /// Получает список моих рабочих элементов. 
        /// </summary>
        /// <param name="type">Тип рабочего элемента. См. <see cref="WorkItemTypes"/></param>
        /// <returns></returns>
        IList<WorkItem> GetMyWorkItems();

        /// <summary>
        /// Создание нового рабочего элемента
        /// </summary>
        /// <param name="title">Заголовок таска</param>
        /// <param name="parent">Рабочий элемент, к которому таск привязан</param>
        /// <param name="hours">Часы, сколько планируем работать</param>
        /// <returns></returns>
        WorkItem CreateTask(string title, WorkItem parent, uint hours);
    }
}
