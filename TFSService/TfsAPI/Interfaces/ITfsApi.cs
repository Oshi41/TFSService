using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;

namespace TfsAPI.Interfaces
{
    public interface ITfsApi
    {
        /// <summary>
        /// Списываю часы в указанный таск
        /// </summary>
        /// <param name="item">Таск, куда списываю</param>
        /// <param name="hours">Кол-во часов</param>
        /// <param name="setActive">Нужно ли выставить состояние таска в Active</param>
        Revision WriteHours(WorkItem item, byte hours, bool setActive);

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
        /// <param name="allowedTypes">Ищем только по указанным типам</param>
        /// <returns></returns>
        IList<WorkItem> Search(string text, params string[] allowedTypes);

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

        /// <summary>
        /// Возвращает список ревизий рабочих элементов с кол-вом их списанных часов
        /// </summary>
        /// <param name="from">Начиная с указанной даты, включая её</param>
        /// <param name="to">Заканчивая указанной датой, включая ей</param>
        /// <returns></returns>
        List<KeyValuePair<Revision, int>> GetCheckins(DateTime from, DateTime to);
    }
}
