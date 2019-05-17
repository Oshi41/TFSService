using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            Trace.WriteLine($"Observing every {minutes} minute(s)");
        }

        #region Event handlers

        private void CheckItems(object sender = null, ElapsedEventArgs e = null)
        {
            var actual = _getMyItems();

            var added = actual.Except(_myItems, _comparer).ToList();
            var removed = _myItems.Except(actual, _comparer).ToList();

            added.ForEach(Subscribe);
            removed.ForEach(Unsubscribe);

            Trace.WriteLineIf(added.Any(), $"Added: {string.Join(", ", added.Select(x => x.Id))}\n");
            Trace.WriteLineIf(removed.Any(), $"Removed: {string.Join(", ", removed.Select(x => x.Id))}\n");

            // В первый раз не надо зажигать никаких событий
            if (!_initialized)
            {
                Trace.WriteLine("First init completed successfully");
                _initialized = true;
                return;
            }

            NewItems?.Invoke(this, added);

            // Todo подумать, что делать с удалёнными объектами

            var notChanged = _myItems.Except(added.Concat(removed), _comparer).ToList();
            notChanged.ForEach(x => x.SyncToLatest());

            // После синхронизации всех рабочих элементов,
            // мы сохранили изменения в своем списке.
            // Зажигаем одно событие для изменения всех
            // затронутых полей всех элементов
            ItemsChanged?.Invoke(this, _fieldChanges);

            // Очищаем список изменений
            _fieldChanges.Clear();
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
            _timer.Dispose();
        }

        public event EventHandler<List<WorkItem>> NewItems;
        public event EventHandler<Dictionary<WorkItem, List<WorkItemEventArgs>>> ItemsChanged;

        public void Start()
        {
            CheckItems();
            _timer.Start();

            Trace.WriteLine("Observing started");
        }

        public void Pause()
        {
            _timer.Stop();

            Trace.WriteLine("Observing paused");
        }

        #endregion
        
    }
}
