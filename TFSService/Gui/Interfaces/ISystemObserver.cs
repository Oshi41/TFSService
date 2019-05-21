using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Gui.Interfaces
{
    public interface ISystemObserver
    {
        /// <summary>
        /// Новые элементы переведены на меня
        /// </summary>
        event EventHandler<List<WorkItem>> ItemsAssigned;

        /// <summary>
        /// Элементы были удалены
        /// </summary>
        event EventHandler<List<WorkItem>> ItemsRemoved;


        ///// <summary>
        ///// пользователь зашёл в систему
        ///// </summary>
        //event EventHandler Login;

        ///// <summary>
        ///// Пользователь вышел из системы
        ///// </summary>
        //event EventHandler Logogff;

        ///// <summary>
        ///// Необходимо списать опр. кол-во часов
        ///// </summary>
        //event EventHandler<int> WriteHours;
    }
}
