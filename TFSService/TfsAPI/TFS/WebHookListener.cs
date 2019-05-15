using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using TfsAPI.Comarers;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace TfsAPI.TFS
{
    public class WebHookListener : IDisposable
    {
        #region Fields

        private readonly Func<IList<WorkItem>> _getMyItems;
        private readonly Dictionary<int, WorkItem> _subscriptions = new Dictionary<int, WorkItem>();
        private readonly Timer _timer;

        #endregion

        #region Events

        /// <summary>
        /// На меня перевели какой-то элемент
        /// </summary>
        public event EventHandler<WorkItem> WorkItemAssigned;

        /// <summary>
        /// Я закрыл/у меня забрали элемент
        /// </summary>
        public event EventHandler<WorkItem> WorkItemRemoved;

        /// <summary>
        /// Рабочий элемент был изменен
        /// </summary>
        public event EventHandler<Field> ItemChanged; 

        #endregion

        public WebHookListener(Func<IList<WorkItem>> getMyItems, byte minutes = 5)
        {
            _getMyItems = getMyItems;
            _timer = new Timer(minutes * 1000);
            _timer.Elapsed += CheckWorkItems;
        }

        /// <summary>
        /// Начинаю мониторинг за элементами
        /// </summary>
        public void Start()
        {
            _subscriptions.Values.ForEach(Unsubscribe);

            InitSubscription(true);
            _timer.Start();
        }

        #region Private methods

        /// <summary>
        /// Подспиываемся на изменение элементов, которые на мне
        /// </summary>
        /// <param name="isInitializing">Первичная инициализация, не нужно говорить пользвоателю, что </param>
        private void InitSubscription(bool isInitializing = false)
        {
            var old = _subscriptions.Values.ToList();
            var actual = _getMyItems();
            var comparer = new WorkItemComparer();

            var added = actual.Except(old, comparer).ToList();
            var removed = old.Except(added, comparer).ToList();

            added.ForEach(Subscribe);
            removed.ForEach(Unsubscribe);

            if (isInitializing)
                return;

            added.ForEach(x => FireItem(x, false));
            removed.ForEach(x => FireItem(x, false));
        }

        private void Subscribe(WorkItem item)
        {
            if (item == null)
                return;

            Unsubscribe(item);

            item.FieldChanged += OnItemChanged;
            _subscriptions[item.Id] = item;
        }

        private void Unsubscribe(WorkItem item)
        {
            if (item == null || !_subscriptions.ContainsKey(item.Id))
                return;

            item.FieldChanged -= OnItemChanged;
            _subscriptions[item.Id].FieldChanged -= OnItemChanged;
            _subscriptions.Remove(item.Id);
        }

        private void FireItem(WorkItem item, bool deleted)
        {
            if (item == null)
                return;

            if (deleted)
            {
                WorkItemRemoved?.Invoke(this, item);
            }
            else
            {
                WorkItemAssigned?.Invoke(this, item);
            }
        }

        #endregion

        #region Event handlers

        private void OnItemChanged(object sender, WorkItemEventArgs e)
        {
            ItemChanged?.Invoke(sender, e.Field);
        }

        private void CheckWorkItems(object sender, ElapsedEventArgs e)
        {
            InitSubscription();
        }

        #endregion

        public void Dispose()
        {
            WorkItemAssigned.Unsubscribe();
            WorkItemRemoved.Unsubscribe();

            _subscriptions.Values.ForEach(Unsubscribe);
            _timer?.Dispose();
        }
    }
}
