using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Gui.Interfaces;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.Win32;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace Gui.Helper
{
    /// <summary>
    /// Класс, обозревающий состояние сессии.
    /// </summary>
    [Obsolete]
    class Observer : ISystemObserver
    {
        #region Fields

        private readonly ITfsApi _tfs;
        private readonly Func<WorkItem> _requestItem;
        private readonly Timer _timer;

        private DateTime _prevDate;

        #endregion

        #region Events

        public event EventHandler<List<WorkItem>> ItemsAssigned;
        public event EventHandler<List<WorkItem>> ItemsRemoved;

        #endregion

        /// <param name="requestItem">Возвращает WorkItem, для которого будут списываться часы </param>
        public Observer(ITfsApi tfs, Func<WorkItem> requestItem)
        {
            _tfs = tfs;
            _requestItem = requestItem;
            using (var settings = Settings.Settings.Read())
            {
                _prevDate = settings.Begin;
            }

            _timer = new Timer(1000 * 60 * 60);
            _timer.Elapsed += (sender, args) => Update(true);


            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.SessionSwitch += OnSessionSwitch;
        }

        #region Event handlers

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            Trace.WriteLine($"Power mode has changed to {e.Mode}");

            // TODO проверить, что при suspend вызывается OnSessionSwitch
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            Trace.WriteLine($"Session state has changed to {e.Reason}");

            switch (e.Reason)
            {
                case SessionSwitchReason.ConsoleConnect:
                case SessionSwitchReason.RemoteConnect:
                case SessionSwitchReason.SessionRemoteControl:
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.SessionUnlock:
                    // Login?.Invoke(this, e);
                    break;

                default:
                    // Logogff?.Invoke(this, e);
                    break;
            }

            Update();
        }

        #endregion

        #region ISystemObserver

        private void Start()
        {
            Update();
            _timer.Start();
        }

        private void Stop()
        {
            _timer.Stop();
        }

        #endregion

        #region Methods

        /// <param name="schedule">Нужно ли записать один час</param>
        private void Update(bool schedule = false)
        {
            using (var settings = Settings.Settings.Read())
            {
                // Начинаем новый рабочий день
                if (!_prevDate.IsToday())
                {
                    TryBeginWorkDay(settings);
                    return;
                }

                if (TryEndWorkDay(settings.CompletedWork))
                {
                    // TODO сообщение об окончании рабочего дня

                    return;
                }

                if (schedule)
                {
                    ScheduleHour(settings);
                }
            }

            SyncMyWorkItems();
        }

        #endregion

        #region Actions

        /// <summary>
        /// Начинаем новый рабочий день
        /// </summary>
        /// <param name="settings"></param>
        private void TryBeginWorkDay(Settings.Settings settings)
        {
            _prevDate = DateTime.Now;
            settings.Begin = _prevDate;

            if (settings.CompletedWork.ScheduledTime() > 0)
            {
                // TODO Спрашиваем у пользователя, если не успели зачекинить рабочее время

                settings.CompletedWork.CheckInWork(_tfs);
            }
            else
            {
                settings.CompletedWork.ClearPrevRecords();
            }

            Trace.WriteLine("Welcome to a new work day!");
        }

        private void ScheduleHour(Settings.Settings settings)
        {
            settings.CompletedWork.SyncCheckins(_tfs);

            var item = _requestItem();
            settings.CompletedWork.ScheduleWork(item.Id, 1);
        }

        private bool TryEndWorkDay(WriteOffCollection work)
        {
            var checkedIn = work.CheckinedTime();
            var scheduled = work.ScheduledTime();
            var capacity = _tfs.GetCapacity();

            // Пользователь начекинил на дневной предел
            if (checkedIn >= capacity)
            {
                Trace.WriteLine($"Capacity is {capacity}, today was checked-in {checkedIn} hour(s)");
                return true;
            }

            // Запланированное и списанное время равно дневному пределу
            if (scheduled + checkedIn >= capacity
                // Либо судя по времени, надо закончить рабочий день
                || DateTime.Now - _prevDate >= TimeSpan.FromHours(capacity))
            {
                work.CheckInWork(_tfs);
                return true;
            }

            return false;
        }

        private void SyncMyWorkItems()
        {
            List<WorkItem> added;
            List<int> removed;

            using (var settings = Settings.Settings.Read())
            {
                var myItems = _tfs.GetMyWorkItems();
                var newIds = myItems.Select(x => x.Id).ToList();

                added = myItems.Where(x => !settings.MyWorkItems.Contains(x.Id)).ToList();
                removed = settings.MyWorkItems.Except(newIds).ToList();

                settings.MyWorkItems = new ObservableCollection<int>(newIds);
            }

            // Вызываем события после записи настроек
            ItemsAssigned?.Invoke(this, added);
            ItemsRemoved?.Invoke(this, removed.Select(x => _tfs.FindById(x)).ToList());
        }

        #endregion
    }
}
