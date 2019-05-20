using System;
using System.Collections.Generic;
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
    class SystemObserver : ISystemObserver
    {
        #region Fields

        private readonly ITfs _tfs;
        private readonly Func<WorkItem> _requestItem;
        private readonly Timer _timer;

        private DateTime _prevDate;

        #endregion

        #region Events

        public event EventHandler Login;
        public event EventHandler Logogff;
        public event EventHandler<int> WriteHours;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tfs"></param>
        /// <param name="requestItem">Возвращает WorkItem, для которого будут списываться часы </param>
        public SystemObserver(ITfs tfs, Func<WorkItem> requestItem)
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
                    Login?.Invoke(this, e);
                    break;

                default:
                    Logogff?.Invoke(this, e);
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

                if (schedule)
                {
                    ScheduleHour(settings);
                }

                if (TryEndWorkDay(settings.CompletedWork))
                {
                    // TODO сообщение об окончании рабочего дня
                }
            }
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
            }
            else
            {
                settings.CompletedWork.Clear();
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
                || DateTime.Now.AddHours(capacity * -1).IsNear(_prevDate, 60))
            {
                work.CheckInWork(_tfs);
                return true;
            }

            return false;
        }

        ///// <summary>
        ///// Записываем в TFS всё, что запомнили
        ///// </summary>
        ///// <param name="collection"></param>
        //private void WriteToTfs(WriteOffCollection collection)
        //{
        //    // Прохожу по всем незаписанным элементам
        //    foreach (var item in collection.Manual)
        //    {
        //        try
        //        {
        //            if (_tfs.FindById(item.Id) is WorkItem task)
        //            {
        //                _tfs.WriteHours(task, (byte)item.Hours, true);
        //            }
        //            else
        //            {
        //                Trace.WriteLine($"Cannot find workitem - {item.Id}");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Trace.WriteLine(e);
        //        }
        //    }

        //    collection.Clear();
        //}

        ///// <summary>
        ///// Записываем все чекины, которые были созданы не программным образом
        ///// </summary>
        ///// <param name="settings"></param>
        //private void SyncWriteOffs(Settings.Settings settings)
        //{
        //    // Получил список чекинов
        //    var checkins = _tfs.GetCheckins(DateTime.Today, DateTime.Now);

        //}

        //private void TryEndWorkDay(Settings.Settings settings)
        //{




        //}

        //private void TryEndWorkDay(Settings.Settings settings)
        //{
        //    var capacity = _tfs.GetCapacity();
        //    var remembered = settings.CompletedWork.GetTotalHours();


        //}

        //private void WriteOffTasks()
        //{
        //    using (var settings = Settings.Settings.Read())
        //    {
        //        // Вытащили то, что не успели списать
        //        var toWrite = settings.CompletedWork;
        //        if (toWrite.IsNullOrEmpty())
        //            return;

        //        while (toWrite.Any())
        //        {
        //            // Вытаскиваем по первому элементу
        //            var workitem = toWrite.First();

        //            // Сразу удаляем
        //            toWrite.Remove(workitem);

        //            // Находим рабочий элемент
        //            var item = _tfs.FindById(workitem.Id);
        //            if (item == null)
        //            {
        //                Trace.WriteLine($"{nameof(SystemObserver)}.{nameof(WriteOffTasks)}:Cannot find task {workitem.Id}");
        //                continue;
        //            }

        //            // И списываем указанное время
        //            _tfs.WriteHours(item, (byte) workitem.Hours, true);
        //        }

        //        // Очищаем список незаписанной работы
        //        settings.CompletedWork.Clear();
        //    }
        //}

        //private void ScheduleWorkHour()
        //{
        //    using (var settings = Settings.Settings.Read())
        //    {
        //        var current = _requestItem?.Invoke();

        //        if (current == null)
        //        {
        //            Trace.WriteLine($"{nameof(SystemObserver)}.{nameof(ScheduleWorkHour)}: Current task is null");
        //            return;
        //        }

        //        settings.CompletedWork.ScheduleWork(current.Id, 1);
        //    }
        //}

        #endregion
    }
}
