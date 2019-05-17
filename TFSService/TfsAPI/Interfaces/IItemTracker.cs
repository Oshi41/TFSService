﻿using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Interfaces
{
    public interface IItemTracker : IDisposable
    {
        /// <summary>
        /// На пользователя назначили новый рабочий элемент
        /// </summary>
        event EventHandler<List<WorkItem>> NewItems;

        /// <summary>
        /// Рабочий элемент был изменен
        /// </summary>
        event EventHandler<Dictionary<WorkItem, List<WorkItemEventArgs>>> ItemsChanged;

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
