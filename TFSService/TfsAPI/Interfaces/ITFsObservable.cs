using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Interfaces
{
    public interface ITFsObservable : ITfsApi, IResumable
    {
        /// <summary>
        /// Пользователь закоммитил изменения
        /// </summary>
        event EventHandler<CommitCheckinEventArgs> Checkin;

        /// <summary>
        /// На пользователя назначили новый рабочий элемент
        /// </summary>
        event EventHandler<List<WorkItem>> NewItems;

        /// <summary>
        /// Рабочий элемент был изменен
        /// </summary>
        event EventHandler<Dictionary<WorkItem, List<WorkItemEventArgs>>> ItemsChanged;

        /// <summary>
        /// Нужно списать время у указанного рабочего элемента
        /// </summary>
        event EventHandler<ScheduleWorkArgs> WriteOff;

        /// <summary>
        /// Юзер вышел из-за компа
        /// </summary>
        event EventHandler Logoff;

        /// <summary>
        /// Юзер залогинился
        /// </summary>
        event EventHandler Logon;

        /// <summary>
        /// Запрашиваем обновление 
        /// </summary>
        void RequestUpdate();
    }

    public class ScheduleWorkArgs : EventArgs
    {
        public ScheduleWorkArgs(WorkItem item, byte hours)
        {
            Item = item;
            Hours = hours;
        }

        public byte Hours { get; }

        public WorkItem Item { get; }
    }
}
