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
        }
        
        #region Methods

        private void Update()
        {
            if (!_prevDate.IsToday())
            {
                // First Initialization

                using (var settings = Settings.Settings.Read())
                {
                    settings.Begin = DateTime.Today;
                    _prevDate = settings.Begin;
                }

                Trace.WriteLine("Welcome to new day!");

                WriteOffPrevDayTasks();
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

        private void WriteOffPrevDayTasks()
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
                    toWrite.Remove(workitem.Key);

                    // Находим рабочий элемент
                    var item = _tfs.FindById(workitem.Key);
                    if (item == null)
                    {
                        Trace.WriteLine($"{nameof(SystemObserver)}.{nameof(WriteOffPrevDayTasks)}:Cannot find task {workitem.Key}");
                        continue;
                    }

                    // И списываем указанное время
                    _tfs.WriteHours(item, (byte) workitem.Value, true);
                }

                // Очищаем список незаписанной работы
                settings.CompletedWork = new Dictionary<int, int>();
            }
        }

        #endregion
    }
}
