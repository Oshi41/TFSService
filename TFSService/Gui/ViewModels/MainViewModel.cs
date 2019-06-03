using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gui.Helper;
using Gui.ViewModels.DialogViewModels;
using Gui.ViewModels.Notifications;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm;
using TfsAPI.Constants;
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
        private StatsViewModel statsViewModel = new StatsViewModel();
        private bool isBusy;

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
                    ApiObservable.ItemsChanged -= OnItemsChanged;
                }

                _apiObserve = value;

                if (ApiObservable != null)
                {
                    ApiObservable.WriteOff += ScheduleHour;
                    ApiObservable.Logoff += OnLogoff;
                    ApiObservable.Logon += OnLogon;
                    ApiObservable.NewItems += OnNewItems;
                    ApiObservable.ItemsChanged += OnItemsChanged;
                }
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
        public CheckinHistoryViewModel MonthScheduleViewModel { get; set; }

        /// <summary>
        /// Основная статистика пользователя
        /// </summary>
        public StatsViewModel StatsViewModel { get => statsViewModel; set => SetProperty(ref statsViewModel, value); }

        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value); }

        #endregion

        #region Commands

        /// <summary>
        /// Подключение к разным TFS (WIP)
        /// </summary>
        public ICommand TfsConnectCommand { get; private set; }

        /// <summary>
        /// Списание моих часов за месяц
        /// </summary>
        public ICommand ShowMonthlyCommand { get; private set; }

        public ICommand UpdateCommand { get; private set; }

        #endregion

        public MainViewModel()
        {
            ShowMonthlyCommand = new ObservableCommand(ShowMonthly);
            UpdateCommand = ObservableCommand.FromAsyncHandler(Update);

            Init();            
        }

        private async void Init()
        {
            IsBusy = true;

            var connect = await TryConnect() == true;

            if (!connect)
            {
                Trace.WriteLine("User denied to connect, exit the program");
                App.Current.Shutdown(0);
            }

            using (var settings = Settings.Settings.Read())
            {
                ApiObservable = new TfsObservable(FirstConnectionViewModel.Text, settings.MyWorkItems, GetTask);
                
                if (!settings.Connections.Contains(FirstConnectionViewModel.Text))
                {
                    settings.Connections.Add(FirstConnectionViewModel.Text);
                }
            }

            TryStartWorkDay();

            // Начинаем наблюдение
            _apiObserve.Start();

            IsBusy = false;
        }

        #region Command handler
        private void ShowMonthly()
        {
            if (MonthScheduleViewModel == null)
            {
                MonthScheduleViewModel = new CheckinHistoryViewModel(_apiObserve);
            }

            WindowManager.ShowDialog(MonthScheduleViewModel, "Месячное списание часов");
        }

        private async Task Update()
        {
            IsBusy = true;

            await Task.Run(() => _apiObserve.RequestUpdate());

            RefreshStats();

            IsBusy = false;
        }

        #endregion

        #region EventHandlers

        private void ScheduleHour(object sender, ScheduleWorkArgs e)
        {
            ScheduleHour(e.Item.Id, e.Hours);

            if (e?.Item != null)
            {
                WindowManager.ShowBaloon(new WriteOffBaloonViewModel(e));
            }
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
            if (!e.IsNullOrEmpty())
            {
                var bugs = e.Where(x => x.IsTypeOf(WorkItemTypes.Bug)).ToList();
                var works = e.Where(x => x.IsTypeOf(WorkItemTypes.Pbi, WorkItemTypes.Improvement)).ToList();
                var rare = e.Where(x => x.IsTypeOf(WorkItemTypes.Incident, WorkItemTypes.Feature)).ToList();
                var responses = e.Where(x => x.IsTypeOf(WorkItemTypes.ReviewResponse)).ToList();
                var requests = e.Where(x => x.IsTypeOf(WorkItemTypes.CodeReview)).ToList();
                var rest = e.Except(bugs).Except(works).Except(rare).Except(responses).Except(requests).ToList();

                if (bugs.Any())
                {
                    WindowManager.ShowBaloon(new ItemsAssignedBaloonViewModel(bugs, "Новые баги"));
                }

                if (works.Any())
                {
                    WindowManager.ShowBaloon(new ItemsAssignedBaloonViewModel(works, "Новая работа"));
                }

                if (rare.Any())
                {
                    WindowManager.ShowBaloon(new ItemsAssignedBaloonViewModel(rare, "Важная вещь, посмотри"));
                }

                if (responses.Any())
                {
                    var vms = new NewResponsesBaloonViewModel(responses, requests, _apiObserve, "Запросили проверку кода");
                }                

                if (rest.Any())
                {
                    WindowManager.ShowBaloon(new ItemsAssignedBaloonViewModel(rest, "Новые рабочие элементы были назначены"));
                }

                RefreshStats();
            }
        }

        private void OnItemsChanged(object sender, Dictionary<WorkItem, List<WorkItemEventArgs>> e)
        {
            if (!e.IsNullOrEmpty())
            {
                RefreshStats();

                // Закрытые элемента
                var closed = e.Where(x => x.Key.HasState(WorkItemStates.Closed));

                // запросы кода
                // var requests = e.Where(x => x.Key.IsTypeOf(WorkItemTypes.ReviewResponse) && x.Key.IsNotClosed());

                foreach (var item in e)
                {

                }
            }
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

                // Нужно 100% выбрать таск
                while (WindowManager.ShowDialog(vm, "Выберите элемент для списания времени", 400, 200) != true)
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

        private void RefreshStats()
        {
            StatsViewModel.Refresh(ApiObservable);
        }

        #endregion

        #region Actions

        private bool TryStartWorkDay()
        {
            using (var settings = Settings.Settings.Read())
            {
                var work = settings.CompletedWork;
                work.SyncCheckins(_apiObserve);

                RefreshStats();

                if (settings.Begin.IsToday())
                {
                    return false;
                }

                settings.Begin = DateTime.Now;                

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

                RefreshStats();
            }
        }

        private bool TryEndWorkDay()
        {
            bool result = false;

            using (var settings = Settings.Settings.Read())
            {
                var now = DateTime.Now;
                var work = settings.CompletedWork;

                // 1) С начала наблюдения уже прошло нужное кол-во часов
                if (now - settings.Begin > TimeSpan.FromHours(settings.Capacity))
                {
                    work.SyncDailyPlan(_apiObserve, settings.Capacity, GetTask);
                    result = true;
                }
                else
                {
                    // 2) Пользователь уже распланировал достаточно времени
                    if (settings.Capacity <= work.ScheduledTime() + work.CheckinedTime())
                    {
                        work.CheckinScheduledWork(_apiObserve, settings.Capacity);
                        result = true;
                    }
                }
                
            }

            RefreshStats();

            return result;
        }

        #endregion
    }
}
