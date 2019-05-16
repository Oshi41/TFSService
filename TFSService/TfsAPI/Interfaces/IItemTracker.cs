using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Interfaces
{
    public interface IItemTracker : IDisposable
    {
        /// <summary>
        /// На пользователя назначили новый рабочий элемент
        /// </summary>
        event EventHandler<WorkItem> NewItem;

        /// <summary>
        /// Рабочий элемент был изменен
        /// </summary>
        event EventHandler<Field> ItemChanged;

        /// <summary>
        /// Стартую наблюдение за рабочими элементами
        /// </summary>
        void Start();

        /// <summary>
        /// Ставлю проверку на паузу
        /// </summary>
        void Pause();
    }
}
