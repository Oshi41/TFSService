using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;

namespace TfsAPI.Interfaces
{
    public interface IWorkItem
    {
        /// <summary>
        ///     Возвращает список прилинкованных рабочих элементов к набору изменений
        /// </summary>
        /// <param name="changeset">ID набора изменений</param>
        /// <returns></returns>
        IList<WorkItem> GetAssociateItems(int changeset);

        /// <summary>
        ///     Ищет рабочий эжлемент по номеру
        /// </summary>
        /// <param name="id">Номер рабочего элемента</param>
        /// <returns></returns>
        WorkItem FindById(int id);

        /// <summary>
        ///     Делаем один запрос на множество элементов
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Dictionary<int, WorkItem> FindById(IEnumerable<int> ids);

        /// <summary>
        ///     Производит поиск в названии, описании
        /// </summary>
        /// <param name="text"></param>
        /// <param name="allowedTypes">Ищем только по указанным типам</param>
        /// <returns></returns>
        IList<WorkItem> Search(string text, params string[] allowedTypes);

        /// <summary>
        ///     Получает список моих рабочих элементов.
        /// </summary>
        /// <param name="type">Тип рабочего элемента. См. <see cref="WorkItemTypes" /></param>
        /// <returns></returns>
        WorkItemCollection GetMyWorkItems();

        /// <summary>
        ///     Запрашиваем рабочие элементы по строке
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        WorkItemCollection QueryItems(string query);

        /// <summary>
        ///     Создание нового рабочего элемента
        /// </summary>
        /// <param name="title">Заголовок таска</param>
        /// <param name="parent">Рабочий элемент, к которому таск привязан</param>
        /// <param name="hours">Часы, сколько планируем работать</param>
        /// <returns></returns>
        WorkItem CreateTask(string title, WorkItem parent, uint hours);

        /// <summary>
        ///     Переданный рабочий элемент ассоциирован со мной
        /// </summary>
        /// <param name="item">Рабочий элемент</param>
        /// <returns></returns>
        bool IsAssignedToMe(WorkItem item);

        /// <summary>
        ///     Получает родителей переданных элементов
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        List<WorkItem> GetParents(params WorkItem[] items);

        /// <summary>
        /// Сохраняем рабочий элемент
        /// </summary>
        /// <param name="item"></param>
        void SaveElement(WorkItem item);

        /// <summary>
        /// Запрос для получения рабочих элементов на мне
        /// </summary>
        string MyItemsQuarry { get; }
    }
}