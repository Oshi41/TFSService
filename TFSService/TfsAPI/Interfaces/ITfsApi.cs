using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;

namespace TfsAPI.Interfaces
{
    /// <summary>
    ///     Можем ли закрыть данную проверку кода
    /// </summary>
    /// <param name="myRequest">Мой запрос на проверку</param>
    /// <param name="responses">Ответы от коллег</param>
    /// <returns></returns>
    public delegate bool CanCloseReview(WorkItem myRequest, IList<WorkItem> responses);

    public interface ITfsApi : IWriteOff, IChekins, IWorkItem
    {
        /// <summary>
        ///     Имя владельца
        /// </summary>
        string Name { get; }
    }
}