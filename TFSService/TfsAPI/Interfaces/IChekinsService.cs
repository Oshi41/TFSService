using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Interfaces
{
    /// <summary>
    ///     Можем ли закрыть данную проверку кода
    /// </summary>
    /// <param name="myRequest">Мой запрос на проверку</param>
    /// <param name="responses">Ответы от коллег</param>
    /// <returns></returns>
    public delegate bool CanCloseReview(WorkItem myRequest, IList<WorkItem> responses);
    
    public interface IChekins
    {
        /// <summary>
        ///     Получаю список чекинов
        /// </summary>
        /// <param name="from">Дата от включительно</param>
        /// <param name="to">Дата до включительно</param>
        /// <param name="user">Имя пользователя. По умолчанию <see langword="null" />, то есть все</param>
        /// <returns></returns>
        List<Changeset> GetCheckins(DateTime from, DateTime to, string user = null);

        /// <summary>
        ///     Закрываю запросы проверки кода, которые проверили
        /// </summary>
        /// <param name="reasons">Можем ли закрыть проверку</param>
        /// <returns></returns>
        List<WorkItem> CloseCompletedReviews(CanCloseReview canRemove);

        /// <summary>
        ///     Кто-то запросил у меня проверку кода, закрываю такие рабочие элементы по предикату
        /// </summary>
        /// <param name="canClose">Можем ли закрыть проверку</param>
        /// <returns></returns>
        List<WorkItem> CloseRequests(Predicate<WorkItem> canClose);
    }
}