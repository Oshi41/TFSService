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
    public class WebHookListener : IItemTracker
    {
        #region Fields

        private readonly Func<IList<WorkItem>> _getMyItems;
        private readonly Dictionary<int, WorkItem> _subscriptions = new Dictionary<int, WorkItem>();
        private readonly Timer _timer;

        private bool _firstInit = true;

        #endregion

        #region Events

        /// <summary>
        /// На меня перевели какой-то элемент
        /// </summary>
        public event EventHandler<WorkItem> NewItem;

        /// <summary>
        /// Я закрыл/у меня забрали элемент
        /// </summary>
        public event EventHandler<WorkItem> WorkItemRemoved;

        /// <summary>
        /// Рабочий элемент был изменен
        /// </summary>
        public event EventHandler<Field> ItemChanged; 

        #endregion

        public WebHookListener(Func<IList<WorkItem>> getMyItems, byte minutes = 1)
        {
            _getMyItems = getMyItems;
            _timer = new Timer(minutes * 1000 * 60);
            _timer.Elapsed += CheckWorkItems;
        }

        #region IItemTracker

        /// <summary>
        /// Начинаю мониторинг за элементами
        /// </summary>
        public void Start()
        {
            InitSubscription();
            _timer.Start();
        }

        public void Pause()
        {
            _timer.Stop();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Подспиываемся на изменение элементов, которые на мне
        /// </summary>
        /// <param name="isInitializing">Первичная инициализация, не нужно говорить пользвоателю, что </param>
        private void InitSubscription()
        {
            var comparer = new WorkItemComparer();

            var old = _subscriptions.Values.ToList();
            var actual = _getMyItems();

            var added = actual.Except(old, comparer).ToList();
            var removed = old.Except(actual, comparer).ToList();

            added.ForEach(Subscribe);
            removed.ForEach(Unsubscribe);

            if (_firstInit)
            {
                _firstInit = false;
                return;
            }

            // Для каждого вызывал событие добавление/удаления
            removed.ForEach(x => FireItem(x, true));
            added.ForEach(x => FireItem(x, false));

            // Синхронизировал нетронутые рабочие элементы
            var notChanged = old.Except(added.Concat(removed), comparer).ToList();
            notChanged.ForEach(x => x.SyncToLatest());
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
                NewItem?.Invoke(this, item);
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
            NewItem.Unsubscribe();
            WorkItemRemoved.Unsubscribe();

            _subscriptions.Values.ForEach(Unsubscribe);
            _timer?.Dispose();
        }
    }
}
