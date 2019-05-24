using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gui.Helper;
using Gui.ViewModels.DialogViewModels;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;
using TfsAPI.TFS;

namespace Gui.ViewModels
{
    public class MainViewModel : BindableBase
    {
        #region Fields

        private ITFsObservable _apiObserve;

        private WorkItemVm _currentTask;

        #endregion

        #region Properties

        private ITFsObservable ApiObservable
        {
            get => _apiObserve;
            set
            {
                if (value == _apiObserve)
                    return;

                if (ApiObservable != null)
                {
                    ApiObservable.WriteOff -= ScheduleHour;
                    ApiObservable.Logoff -= OnLogoff;
                    ApiObservable.Logon -= OnLogon;
                    ApiObservable.NewItems -= OnNewItems;
                }

                _apiObserve = value;

                if (ApiObservable != null)
                {
                    ApiObservable.WriteOff += ScheduleHour;
                    ApiObservable.Logoff += OnLogoff;
                    ApiObservable.Logon += OnLogon;
                    ApiObservable.NewItems += OnNewItems;
                }

                //if (SetProperty(ref _apiObserve, value) 
                //    && ApiObservable != null)
                //{
                //    ApiObservable.WriteOff += ScheduleHour;
                //    ApiObservable.Logoff += OnLogoff;
                //}
            }
        }

        

        /// <summary>
        /// Диалог запроса рабочего элемента, над которым работаем
        /// </summary>
        public ChooseTaskViewModel ChooseTaskViewModel { get; private set; }

        /// <summary>
        /// Строка подключения к TFS
        /// </summary>
        public FirstConnectionViewModel FirstConnectionViewModel { get; set; }

        /// <summary>
        /// Создание рабочего элемента
        /// </summary>
        public CreateTaskViewModel CreateTaskViewModel { get; set; }

        /// <summary>
        /// Расписание моих месячных трудозатрат
        /// </summary>
        public MonthScheduleViewModel MonthScheduleViewModel { get; set; }

        #endregion

        public MainViewModel()
        {
            Init();
        }

        private async void Init()
        {
            var connect = await TryConnect() == true;

            if (!connect)
            {
                Trace.WriteLine("User denied to connect, exit the program");
                App.Current.Shutdown(0);
            }

            using (var settings = Settings.Settings.Read())
            {
                ApiObservable = new TfsObservable(FirstConnectionViewModel.Text, settings.MyWorkItems, GetTask);
            }

            TryStartWorkDay();

            // Начинаем наблюдение
            _apiObserve.Start();
        }

        #region EventHandlers

        private void ScheduleHour(object sender, ScheduleWorkArgs e)
        {
            ScheduleHour(e.Item.Id, e.Hours);
        }

        private void OnLogoff(object sender, EventArgs e)
        {
            if (TryEndWorkDay())
            {
                _apiObserve.Pause();
            }
        }

        private void OnLogon(object sender, EventArgs e)
        {
            if (TryStartWorkDay())
            {
                _apiObserve.Start();
            }
        }

        private void OnNewItems(object sender, List<WorkItem> e)
        {
            
        }

        #endregion

        #region Methods

        /// <summary>
        /// Первый шаг - подключение к TFS
        /// </summary>
        /// <returns></returns>
        private async Task<bool?> TryConnect()
        {
            FirstConnectionViewModel = new FirstConnectionViewModel();

            if (FirstConnectionViewModel.RememberedConnections.Count == 1
                && FirstConnectionViewModel.CanConnect())
            {
                await FirstConnectionViewModel.Connect();
                return FirstConnectionViewModel.IsConnected;
            }

            return WindowManager.ShowDialog(FirstConnectionViewModel, "Первое подключение", 400, 200);
        }

        /// <summary>
        /// Таск, с которого списываем время. Строго not null!
        /// </summary>
        /// <returns></returns>
        private WorkItem GetTask()
        {
            // Обновляем наблюдаемый рабочий элемент
            _currentTask?.Item?.SyncToLatest();

            if (_currentTask == null || !IsTaskAvailable())
            {
                var vm = new ChooseTaskViewModel(_apiObserve);

                if (WindowManager.ShowDialog(vm, "Выберите элемент для списания времени", 400, 200) != true)
                {
                    Trace.WriteLine("User denied to choose task");
                }

                _currentTask = vm.Searcher.Selected;
            }

            return _currentTask;
        }

        private bool IsTaskAvailable()
        {
            if (!_currentTask.Item.IsTaskAvailable())
            {
                Trace.WriteLine($"{nameof(MainViewModel)}: Task {_currentTask?.Item?.Id} is not exist or has been closed");
                return false;
            }

            if (!_apiObserve.IsAssignedToMe(_currentTask))
            {
                Trace.WriteLine($"{nameof(MainViewModel)}: Task is not assigned to me");
            }

            return true;
        }

        #endregion

        #region Actions

        private bool TryStartWorkDay()
        {
            using (var settings = Settings.Settings.Read())
            {
                if (settings.Begin.IsToday())
                {
                    return false;
                }

                settings.Begin = DateTime.Now;

                var work = settings.CompletedWork;

                // Что-то не зачекинили с утра
                if (work.ScheduledTime() != 0)
                {
                    work.CheckinScheduledWork(_apiObserve, settings.Capacity);
                }

                work.ClearPrevRecords();

                // Выставлили сколько надо списать часов сегодня
                settings.Capacity = _apiObserve.GetCapacity();

                Trace.WriteLine($"{settings.Begin.ToShortTimeString()}: Welcome to a new day!");
            }

            return true;
        }

        private void ScheduleHour(int id, byte hours)
        {
            using (var settigs = Settings.Settings.Read())
            {
                settigs.CompletedWork.ScheduleWork(id, hours);

                // Сразу же списываем час работы
                settigs.CompletedWork.CheckinScheduledWork(_apiObserve, settigs.Capacity);
            }
        }

        private bool TryEndWorkDay()
        {
            using (var settings = Settings.Settings.Read())
            {
                var now = DateTime.Now;
                var work = settings.CompletedWork;

                // 1) С начала наблюдения уже прошло нужное кол-во часов
                if (now - settings.Begin > TimeSpan.FromHours(settings.Capacity))
                {
                    work.SyncDailyPlan(_apiObserve, settings.Capacity, GetTask);
                    return true;
                }

                // 2) Пользователь уже распланировал достаточно времени
                if (settings.Capacity <= work.ScheduledTime() + work.CheckinedTime())
                {
                    work.CheckinScheduledWork(_apiObserve, settings.Capacity);
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
