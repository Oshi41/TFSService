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
    class SystemObserver : ISystemObserver
    {
        #region Fields

        private readonly ITfs _tfs;
        private readonly Func<WorkItem> _requestItem;
        private readonly int _capacity;
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
                _capacity = settings.Capacity;
                _prevDate = settings.Begin;
            }

            _timer = new Timer(1000 * 60 * 60);
            _timer.Elapsed += (sender, args) => Update();

            SystemEvents.PowerModeChanged += OnPowerModeChanged;
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {

        }

        #region Methods

        private void Update()
        {
            using (var settings = Settings.Settings.Read())
            {
                // Начинаем новый рабочий день
                if (!_prevDate.IsToday())
                {
                    TryBeginWorkDay(settings);
                    return;
                }

                //// Часы, накопленные программой
                //
                //

                //// Заканчиваем рабочий день, т.к. записали все
                //if (remembered >= capacity)
                //{
                //    WriteOffTasks();
                //    return;
                //}


                //// Нужно записать час работы
                //ScheduleWorkHour();
            }
        }

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

        #region Actions

        private void TryBeginWorkDay(Settings.Settings settings)
        {
            _prevDate = DateTime.Now;
            settings.Begin = _prevDate;
            // Сбрасываем историю учёта времени
            settings.CompletedWork.Clear();

            Trace.WriteLine("Welcome to a new work day!");
        }

        private void TryEndWorkDay(Settings.Settings settings)
        {
            var capacity = _tfs.GetCapacity();
            var remembered = settings.CompletedWork.GetTotalHours();


        }

        private void WriteOffTasks()
        {
            using (var settings = Settings.Settings.Read())
            {
                // Вытащили то, что не успели списать
                var toWrite = settings.CompletedWork;
                if (toWrite.IsNullOrEmpty())
                    return;

                while (toWrite.Any())
                {
                    // Вытаскиваем по первому элементу
                    var workitem = toWrite.First();

                    // Сразу удаляем
                    toWrite.Remove(workitem);

                    // Находим рабочий элемент
                    var item = _tfs.FindById(workitem.Id);
                    if (item == null)
                    {
                        Trace.WriteLine($"{nameof(SystemObserver)}.{nameof(WriteOffTasks)}:Cannot find task {workitem.Id}");
                        continue;
                    }

                    // И списываем указанное время
                    _tfs.WriteHours(item, (byte) workitem.Hours, true);
                }

                // Очищаем список незаписанной работы
                settings.CompletedWork.Clear();
            }
        }

        private void ScheduleWorkHour()
        {
            using (var settings = Settings.Settings.Read())
            {
                var current = _requestItem?.Invoke();

                if (current == null)
                {
                    Trace.WriteLine($"{nameof(SystemObserver)}.{nameof(ScheduleWorkHour)}: Current task is null");
                    return;
                }

                settings.CompletedWork.ScheduleWork(current.Id, 1);
            }
        }

        #endregion
    }
}
