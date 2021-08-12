using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.TFS;
using TfsAPI.TFS.Trend;

namespace TfsAPI.Interfaces
{
    public interface IWriteOff
    {
        /// <summary>
        ///     Списываю часы в указанный таск
        /// </summary>
        /// <param name="item">Таск, куда списываю</param>
        /// <param name="hours">Кол-во часов</param>
        /// <param name="setActive">Нужно ли выставить состояние таска в Active</param>
        Revision WriteHours(WorkItem item, byte hours, bool setActive);

        /// <summary>
        ///     Возвращает кол-во часов, которое необходимо списать за день
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
        ///     Возвращает список ревизий рабочих элементов с кол-вом их списанных часов
        /// </summary>
        /// <param name="from">Начиная с указанной даты, включая её</param>
        /// <param name="to">Заканчивая указанной датой, включая ей</param>
        /// <returns></returns>
        List<KeyValuePair<Revision, int>> GetWriteoffs(DateTime from, DateTime to);

        /// <summary>
        /// Запрашивает график трудозатрат на указанный месяц (до конца или до сегодня)
        /// </summary>
        /// <param name="from">Начиная с даты</param>
        /// <param name="to">Заканчивая датой</param>
        /// <param name="dailyCapacity">Сколько работаешь в день</param>
        /// <returns></returns>
        Chart GetForMonth(DateTime from, DateTime to, int dailyCapacity);
        
        string Name { get; }
    }
}