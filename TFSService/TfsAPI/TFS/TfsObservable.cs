using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.Win32;
using TfsAPI.Comarers;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace TfsAPI.TFS
{
    public class TfsObservable : TfsApi, ITFsObservable
    {
        public TfsObservable(string url,
            IList<int> myItems,
            Func<WorkItem> currentItem,
            int itemCheck)
            : base(url)
        {
            _currentItem = currentItem;
            _versionControl = _project.GetService<VersionControlServer>();
            _hourTimer = new Timer(1000 * 60 * 60);
            _hourTimer.Elapsed += (sender, args) => RequestUpdate(true);

            _itemsTimer = new Timer(1000 * 60 * itemCheck);
            _hourTimer.Elapsed += (sender, args) => RequestUpdate();

            SystemEvents.SessionSwitch += OnSessionSwitched;
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => Logoff?.Invoke(this, e);

            MyItems = new List<WorkItem>(FindById(myItems).Values);

            _cache = new MemoryCache(new MemoryCacheOptions());
            _searcher = new CachedCapacitySearcher(_cache, _project);
        }

        private IEnumerable<WorkItem> MyItems
        {
            get => _myItems;
            set
            {
                if (Equals(MyItems, value))
                    return;

                if (!MyItems.IsNullOrEmpty())
                    MyItems.ForEach(x => x.FieldChanged -= RememberChange);

                _myItems = value.ToList();

                if (!MyItems.IsNullOrEmpty())
                    MyItems.ForEach(x => x.FieldChanged += RememberChange);
            }
        }

        private async void RequestUpdate(bool timerEllapsed)
        {
            // Находимся в режиме ожилания
            if (_paused) return;

            // По запросу таймера спишем время
            if (timerEllapsed) WriteOff?.Invoke(this, new ScheduleWorkArgs(_currentItem(), 1));

            var current = GetMyWorkItems();

            var added = current.Except(MyItems, _comparer).ToList();
            var removed = MyItems.Except(current, _comparer).ToList();

            if (added.Any()) NewItems?.Invoke(this, added.ToList());

            if (removed.Any())
            {
                // TODO подумать что с ними делать
            }

            // Обновляю оставшиеся, они сыпят миллионы событий,
            // я их собираю и потом выбрасываю одним ивентом
            await System.Threading.Tasks.Task.Run(() =>
            {
                MyItems
                .Except(removed, _comparer)
                .AsParallel()
                .ForEach(x => x.SyncToLatest());
            });

            // Записываю новые значения
            MyItems = current.ToList();

            if (_changes.Any())
            {
                ItemsChanged?.Invoke(this, _changes);

                _changes.Clear();
            }
        }

        #region Fields

        private readonly VersionControlServer _versionControl;
        private readonly Timer _hourTimer;
        private readonly Timer _itemsTimer;
        private readonly Func<WorkItem> _currentItem;
        private readonly IEqualityComparer<WorkItem> _comparer = new WorkItemComparer();

        private readonly Dictionary<WorkItem, List<WorkItemEventArgs>> _changes =
            new Dictionary<WorkItem, List<WorkItemEventArgs>>();

        private bool _paused = true;
        private List<WorkItem> _myItems = new List<WorkItem>();

        private readonly MemoryCache _cache;
        private readonly CachedCapacitySearcher _searcher;

        #endregion

        #region ITFsObservable

        public event EventHandler<CommitCheckinEventArgs> Checkin;
        public event EventHandler<List<WorkItem>> NewItems;
        public event EventHandler<Dictionary<WorkItem, List<WorkItemEventArgs>>> ItemsChanged;
        public event EventHandler<ScheduleWorkArgs> WriteOff;
        public event EventHandler Logoff;
        public event EventHandler Logon;

        public void Start()
        {
            // Уже работаем
            if (!_paused)
                return;

            _paused = false;
            RequestUpdate();

            _versionControl.CommitCheckin += OnCheckIn;
            _hourTimer.Start();
            _itemsTimer.Start();
        }

        public void Pause()
        {
            // Уже остановлены
            if (_paused)
                return;

            _paused = true;

            _versionControl.CommitCheckin -= OnCheckIn;
            _hourTimer.Stop();
            _itemsTimer.Stop();
        }

        public void RequestUpdate()
        {
            RequestUpdate(false);
        }

        #endregion

        #region Event handlers

        private void OnCheckIn(object sender, CommitCheckinEventArgs e)
        {
            Trace.WriteLine($"Changeset {e.ChangesetId} was made");

            Checkin?.Invoke(this, e);
        }

        private void OnSessionSwitched(object sender, SessionSwitchEventArgs e)
        {
            Trace.WriteLine($"Session switched to {e.Reason}");

            switch (e.Reason)
            {
                case SessionSwitchReason.ConsoleConnect:
                case SessionSwitchReason.RemoteConnect:
                case SessionSwitchReason.SessionRemoteControl:
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.SessionUnlock:
                    Logon?.Invoke(this, e);
                    break;

                default:
                    Logoff?.Invoke(this, e);
                    break;
            }
        }

        private void RememberChange(object sender, WorkItemEventArgs e)
        {
            if (e?.Field?.WorkItem == null)
                return;

            if (_changes.ContainsKey(e.Field.WorkItem))
                _changes[e.Field.WorkItem].Add(e);
            else
                _changes[e.Field.WorkItem] = new List<WorkItemEventArgs> { e };
        }

        #endregion

        #region Override

        private const string _myWorkItemsKey = "myItemsCachePolicyName";

        public override IList<WorkItem> GetMyWorkItems()
        {
            // 
            // Сделано хэширование, чтобы не делать запросы чаще, чем раз в минуту
            //

            if (!_cache.TryGetValue<IList<WorkItem>>(_myWorkItemsKey, out var result))
            {
                result = base.GetMyWorkItems();

                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));

                lock (_myWorkItemsKey)
                {
                    _cache.Set(_myWorkItemsKey, result, options);
                }
            }

            return result;
        }

        public override List<TeamCapacity> GetCapacity(DateTime start, DateTime end)
        {
            return _searcher.SearchCapacities(Name, start, end);
        }

        #endregion
    }
}