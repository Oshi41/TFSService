using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Interfaces
{
    public class ItemsChanged : EventArgs
    {
        public ItemsChanged(CollectionChangeAction action, IList<WorkItem> items)
        {
            Action = action;
            Items = items;
        }

        /// <summary>
        /// Список изменённых элементов.
        /// </summary>
        public IList<WorkItem> Items { get; }
        
        /// <summary>
        /// Тип операции
        /// </summary>
        public CollectionChangeAction Action { get; }
    }
    
    public interface IWorkItemObserver : ICollectionObserver<WorkItem>
    {
        /// <summary>
        /// Состояние сборки изменилось
        /// </summary>
        event EventHandler<ItemsChanged> ItemsChanged;
    }
}