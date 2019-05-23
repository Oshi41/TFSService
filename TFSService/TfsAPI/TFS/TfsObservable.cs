using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.Win32;
using TfsAPI.Comarers;
using TfsAPI.Interfaces;

namespace TfsAPI.TFS
{
    public class TfsObservable : TfsApi, ITFsObservable
    {
        #region Fields
        private readonly VersionControlServer _versionControl;
        private readonly Timer _timer;
        private readonly Func<WorkItem> _currentItem;
        private readonly IEqualityComparer<WorkItem> _comparer = new WorkItemComparer();
        private readonly Dictionary<WorkItem, List<WorkItemEventArgs>> _changes =
            new Dictionary<WorkItem, List<WorkItemEventArgs>>();

        private bool _paused;
        private List<WorkItem> _myItems = new List<WorkItem>();

        #endregion

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

        public TfsObservable(string url,
            IList<int> myItems,
            Func<WorkItem> currentItem)
            : base(url)
        {
            _currentItem = currentItem;
            _versionControl = _project.GetService<VersionControlServer>();
            _timer = new Timer(1000 * 60 * 60);
            _timer.Elapsed += (sender, args) => RequestUpdate(true);

            SystemEvents.SessionSwitch += OnSessionSwitched;

            MyItems = new List<WorkItem>(myItems.Select(FindById).Where(x => x != null));
        }

        #region ITFsObservable

        public event EventHandler<CommitCheckinEventArgs> Checkin;
        public event EventHandler<List<WorkItem>> NewItems;
        public event EventHandler<Dictionary<WorkItem, List<WorkItemEventArgs>>> ItemsChanged;
        public event EventHandler<ScheduleWorkArgs> WriteOff;
        public event EventHandler Logoff;
        public event EventHandler Logon;

        public void Start()
        {
            _paused = false;

            _versionControl.CommitCheckin += OnCheckIn;
            _timer.Start();
        }

        public void Pause()
        {
            _paused = true;

            _versionControl.CommitCheckin -= OnCheckIn;
            _timer.Stop();
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
            if (_changes.ContainsKey(e.Field.WorkItem))
            {
                _changes[e.Field.WorkItem].Add(e);
            }
            else
            {
                _changes[e.Field.WorkItem] = new List<WorkItemEventArgs> {e};
            }
        }

        #endregion

        private void RequestUpdate(bool timerEllapsed)
        {
            // Находимся в режиме ожилания
            if (_paused) return;

            // По запросу таймера спишем время
            if (timerEllapsed)
            {
                WriteOff?.Invoke(this, new ScheduleWorkArgs(_currentItem(), 1));
            }

            var current = GetMyWorkItems();

            var added = current.Except(MyItems, _comparer).ToList();
            var removed = MyItems.Except(current, _comparer).ToList();

            if (added.Any())
            {
                NewItems?.Invoke(this, added.ToList());
            }

            if (removed.Any())
            {
                // TODO подумать что с ними делать
            }

            // Обновляю оставшиеся, они сыпят миллионы событий,
            // я их собираю и потом выбрасываю одним ивентом
            MyItems.Except(removed, _comparer).ForEach(x => x.SyncToLatest());

            // Записываю новые значения
            MyItems = current.ToList();

            if (_changes.Any())
            {
                ItemsChanged?.Invoke(this, _changes);

                _changes.Clear();
            }
        }
    }
}
