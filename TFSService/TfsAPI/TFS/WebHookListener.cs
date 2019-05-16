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

        /// <summary>
        /// Список объектов на мне
        /// </summary>
        private readonly List<WorkItem> _myItems = new List<WorkItem>();

        /// <summary>
        /// Список изменений
        /// </summary>
        private readonly Dictionary<WorkItem, List<WorkItemEventArgs>> _fieldChanges = new Dictionary<WorkItem, List<WorkItemEventArgs>>();

        private readonly IEqualityComparer<WorkItem> _comparer = new WorkItemComparer();


        private readonly Timer _timer;
        private readonly Func<IList<WorkItem>> _getMyItems;

        /// <summary>
        /// Было ли запущено наблюдение
        /// </summary>
        private bool _initialized;

        #endregion

        public WebHookListener(Func<IList<WorkItem>> getMyItems, byte minutes = 1)
        {
            _getMyItems = getMyItems;
            _timer = new Timer(1000 * 60 * minutes);
            _timer.Elapsed += CheckItems;
        }

        #region Event handlers

        private void CheckItems(object sender = null, ElapsedEventArgs e = null)
        {
            var actual = _getMyItems();

            var added = actual.Except(_myItems, _comparer).ToList();
            var removed = _myItems.Except(actual, _comparer).ToList();

            added.ForEach(Subscribe);
            removed.ForEach(Unsubscribe);

            if (!_initialized)
            {
                _initialized = true;
                return;
            }


            var notChanged = _myItems.Except(added.Concat(removed), _comparer).ToList();
            notChanged.ForEach(x => x.SyncToLatest());
        }

        /// <summary>
        /// Запоминаем изменения 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFieldChanged(object sender, WorkItemEventArgs e)
        {
            var key = e.Field.WorkItem;

            if (_fieldChanges.ContainsKey(key))
            {
                _fieldChanges[key].Add(e);
            }
            else
            {
                _fieldChanges[key] = new List<WorkItemEventArgs> {e};
            }
        }

        #endregion

        #region Methods

        private void Unsubscribe(WorkItem obj)
        {
            obj.FieldChanged -= OnFieldChanged;
        }

        private void Subscribe(WorkItem obj)
        {
            obj.FieldChanged += OnFieldChanged;
        }

        #endregion

        #region IItemTracker

        public void Dispose()
        {
            
        }

        public event EventHandler<WorkItem> NewItem;
        public event EventHandler<Field> ItemChanged;

        public void Start()
        {
            CheckItems();
            _timer.Start();
        }

        public void Pause()
        {
            _timer.Stop();
        }

        #endregion

        //#region Fields

        //private readonly Func<IList<WorkItem>> _getMyItems;
        //private readonly Dictionary<int, WorkItem> _subscriptions = new Dictionary<int, WorkItem>();
        //private readonly Timer _timer;

        //private bool _firstInit = true;

        //#endregion

        //#region Events

        ///// <summary>
        ///// На меня перевели какой-то элемент
        ///// </summary>
        //public event EventHandler<WorkItem> NewItem;

        ///// <summary>
        ///// Я закрыл/у меня забрали элемент
        ///// </summary>
        //public event EventHandler<WorkItem> WorkItemRemoved;

        ///// <summary>
        ///// Рабочий элемент был изменен
        ///// </summary>
        //public event EventHandler<Field> ItemChanged; 

        //#endregion

        //public WebHookListener(Func<IList<WorkItem>> getMyItems, byte minutes = 1)
        //{
        //    _getMyItems = getMyItems;
        //    _timer = new Timer(minutes * 1000 * 60);
        //    _timer.Elapsed += CheckWorkItems;
        //}

        //#region IItemTracker

        ///// <summary>
        ///// Начинаю мониторинг за элементами
        ///// </summary>
        //public void Start()
        //{
        //    InitSubscription();
        //    _timer.Start();
        //}

        //public void Pause()
        //{
        //    _timer.Stop();
        //}

        //#endregion

        //#region Private methods

        ///// <summary>
        ///// Подспиываемся на изменение элементов, которые на мне
        ///// </summary>
        ///// <param name="isInitializing">Первичная инициализация, не нужно говорить пользвоателю, что </param>
        //private void InitSubscription()
        //{
        //    var comparer = new WorkItemComparer();

        //    var old = _subscriptions.Values.ToList();
        //    var actual = _getMyItems();




        //    if (_firstInit)
        //    {
        //        _firstInit = false;
        //        return;
        //    }

        //    // Для каждого вызывал событие добавление/удаления
        //    removed.ForEach(x => FireItem(x, true));
        //    added.ForEach(x => FireItem(x, false));

        //    // Синхронизировал нетронутые рабочие элементы
        //    
        //    notChanged.ForEach(x => x.SyncToLatest());
        //}

        //private void Subscribe(WorkItem item)
        //{
        //    if (item == null)
        //        return;

        //    Unsubscribe(item);

        //    item.FieldChanged += OnItemChanged;
        //    _subscriptions[item.Id] = item;
        //}

        //private void Unsubscribe(WorkItem item)
        //{
        //    if (item == null || !_subscriptions.ContainsKey(item.Id))
        //        return;

        //    item.FieldChanged -= OnItemChanged;
        //    _subscriptions[item.Id].FieldChanged -= OnItemChanged;
        //    _subscriptions.Remove(item.Id);
        //}

        //private void FireItem(WorkItem item, bool deleted)
        //{
        //    if (item == null)
        //        return;

        //    if (deleted)
        //    {
        //        WorkItemRemoved?.Invoke(this, item);
        //    }
        //    else
        //    {
        //        NewItem?.Invoke(this, item);
        //    }
        //}

        //#endregion

        //#region Event handlers

        //private void OnItemChanged(object sender, WorkItemEventArgs e)
        //{
        //    ItemChanged?.Invoke(sender, e.Field);
        //}

        //private void CheckWorkItems(object sender, ElapsedEventArgs e)
        //{
        //    InitSubscription();
        //}

        //#endregion

        //public void Dispose()
        //{
        //    NewItem.Unsubscribe();
        //    WorkItemRemoved.Unsubscribe();

        //    _subscriptions.Values.ForEach(Unsubscribe);
        //    _timer?.Dispose();
        //}
        
    }
}
