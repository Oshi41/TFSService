using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;
using TfsAPI.TFS;

namespace TfsAPI.Interfaces
{
    /// <summary>
    ///     Можем ли закрыть данную проверку кода
    /// </summary>
    /// <param name="myRequest">Мой запрос на проверку</param>
    /// <param name="responses">Ответы от коллег</param>
    /// <returns></returns>
    public delegate bool CanCloseReview(WorkItem myRequest, IList<WorkItem> responses);

    public interface ITfsApi
    {
        /// <summary>
        ///     Имя владельца
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Списываю часы в указанный таск
        /// </summary>
        /// <param name="item">Таск, куда списываю</param>
        /// <param name="hours">Кол-во часов</param>
        /// <param name="setActive">Нужно ли выставить состояние таска в Active</param>
        Revision WriteHours(WorkItem item, byte hours, bool setActive);

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
        ///     Возвращает кол-во часов, которое необходимо списать за день
        ///     TODO переменовать в GetCurrentCapacity либо перегрузить метод с передачей дат
        /// </summary>
        /// <returns></returns>
        int GetCapacity();

        /// <summary>
        ///     Возвращает список
        /// </summary>
        /// <param name="start"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        List<TeamCapacity> GetCapacity(DateTime start, DateTime end);

        /// <summary>
        ///     Получает список моих рабочих элементов.
        /// </summary>
        /// <param name="type">Тип рабочего элемента. См. <see cref="WorkItemTypes" /></param>
        /// <returns></returns>
        IList<WorkItem> GetMyWorkItems();

        /// <summary>
        ///     Запрашиваем рабочие элементы по строке
        /// </summary>
        /// <param name="additionalQuery"></param>
        /// <returns></returns>
        IList<WorkItem> QueryItems(string additionalQuery);

        /// <summary>
        ///     Создание нового рабочего элемента
        /// </summary>
        /// <param name="title">Заголовок таска</param>
        /// <param name="parent">Рабочий элемент, к которому таск привязан</param>
        /// <param name="hours">Часы, сколько планируем работать</param>
        /// <returns></returns>
        WorkItem CreateTask(string title, WorkItem parent, uint hours);

        /// <summary>
        ///     Возвращает список ревизий рабочих элементов с кол-вом их списанных часов
        /// </summary>
        /// <param name="from">Начиная с указанной даты, включая её</param>
        /// <param name="to">Заканчивая указанной датой, включая ей</param>
        /// <returns></returns>
        List<KeyValuePair<Revision, int>> GetWriteoffs(DateTime from, DateTime to);

        /// <summary>
        ///     Получаю список чекинов
        /// </summary>
        /// <param name="from">Дата от включительно</param>
        /// <param name="to">Дата до включительно</param>
        /// <param name="user">Имя пользователя. По умолчанию <see langword="null" />, то есть все</param>
        /// <returns></returns>
        List<Changeset> GetCheckins(DateTime from, DateTime to, string user = null);

        /// <summary>
        ///     Переданный рабочий элемент ассоциирован со мной
        /// </summary>
        /// <param name="item">Рабочий элемент</param>
        /// <returns></returns>
        bool IsAssignedToMe(WorkItem item);

        /// <summary>
        ///     Закрываю запросы проверки кода, которые проверили
        /// </summary>
        /// <param name="reasons">Можем ли закрыть проверку</param>
        /// <returns></returns>
        List<WorkItem> CloseCompletedReviews(CanCloseReview canRemove);

        /// <summary>
        ///     Получает родителей переданных элементов
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        List<WorkItem> GetParents(params WorkItem[] items);
    }
}