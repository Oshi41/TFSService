using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Gui.Helper;
using Gui.Settings;
using Gui.ViewModels.DialogViewModels;
using Gui.ViewModels.Notifications;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.VersionControl.Common.Internal;
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
        public MainViewModel()
        {
            _safeExecutor = new SafeExecutor();

            ShowMonthlyCommand = new ObservableCommand(ShowMonthly);
            UpdateCommand = ObservableCommand.FromAsyncHandler(Update);
            SettingsCommand = new ObservableCommand(ShowSettings);

            Init();
        }

        private async void Init()
        {
            IsBusy = true;

            var connect = await TryConnect() == true;

            if (!connect)
            {
                Trace.WriteLine("User denied to connect, exit the program");
                Application.Current.Shutdown(0);
                return;
            }

            using (var settings = Settings.Settings.Read())
            {
                ApiObservable = new TfsObservable(FirstConnectionViewModel.Text, settings.MyWorkItems, GetTask, settings.ItemMinutesCheck);

                if (!settings.Connections.Contains(FirstConnectionViewModel.Text))
                    settings.Connections.Add(FirstConnectionViewModel.Text);
            }

            TryStartWorkDay();

            // Начинаем наблюдение
            _apiObserve.Start();

            IsBusy = false;
        }

        #region Fields

        private readonly SafeExecutor _safeExecutor;

        private ITFsObservable _apiObserve;

        private WorkItemVm _currentTask;
        private StatsViewModel statsViewModel = new StatsViewModel();
        private bool isBusy;
        private NewResponsesBaloonViewModel codeResponsesViewModel;

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
        ///     Диалог запроса рабочего элемента, над которым работаем
        /// </summary>
        public ChooseTaskViewModel ChooseTaskViewModel { get; private set; }

        /// <summary>
        ///     Строка подключения к TFS
        /// </summary>
        public FirstConnectionViewModel FirstConnectionViewModel { get; set; }

        /// <summary>
        ///     Создание рабочего элемента
        /// </summary>
        public CreateTaskViewModel CreateTaskViewModel { get; set; }

        /// <summary>
        ///     Основная статистика пользователя
        /// </summary>
        public StatsViewModel StatsViewModel
        {
            get => statsViewModel;
            set => SetProperty(ref statsViewModel, value);
        }

        public NewResponsesBaloonViewModel CodeResponsesViewModel
        {
            get => codeResponsesViewModel;
            set => SetProperty(ref codeResponsesViewModel, value);
        }

        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Подключение к разным TFS (WIP)
        /// </summary>
        public ICommand TfsConnectCommand { get; private set; }

        /// <summary>
        ///     Списание моих часов за месяц
        /// </summary>
        public ICommand ShowMonthlyCommand { get; }

        public ICommand UpdateCommand { get; }
        public ICommand SettingsCommand { get; }

        #endregion

        #region Command handler

        private void ShowMonthly()
        {
            WindowManager.ShowDialog(new MonthCheckinsViewModel(_apiObserve), Properties.Resources.AS_MonthlySchedule, 700, 500);
        }

        private async Task Update()
        {
            IsBusy = true;

            await Task.Run(() => _apiObserve.RequestUpdate());

            RefreshStats();

            IsBusy = false;
        }

        private void ShowSettings()
        {
            var vm = new SettingsViewModel(FirstConnectionViewModel.Text, _apiObserve);

            WindowManager.ShowDialog(vm, Properties.Resources.AS_Settings, 500);
        }

        #endregion

        #region EventHandlers

        private void ScheduleHour(object sender, ScheduleWorkArgs e)
        {
            ScheduleHour(e.Item.Id, e.Hours);

            if (e?.Item != null) WindowManager.ShowBaloon(new WriteOffBaloonViewModel(e));
        }

        private void OnLogoff(object sender, EventArgs e)
        {
            if (TryEndWorkDay()) _apiObserve.Pause();
        }

        private void OnLogon(object sender, EventArgs e)
        {
            if (TryStartWorkDay()) _apiObserve.Start();
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

                if (bugs.Any()) WindowManager.ShowBaloon(new ItemsAssignedBaloonViewModel(bugs, Properties.Resources.AS_NewBugs));

                if (works.Any()) WindowManager.ShowBaloon(new ItemsAssignedBaloonViewModel(works, Properties.Resources.AS_NewWork));

                if (rare.Any())
                    WindowManager.ShowBaloon(new ItemsAssignedBaloonViewModel(rare, Properties.Resources.AS_ImportantNotice));

                if (responses.Any())
                {
                    var vms = new NewResponsesBaloonViewModel(responses, requests, _apiObserve, Properties.Resources.AS_CodeReviewRequested);
                }

                if (rest.Any())
                    WindowManager.ShowBaloon(
                        new ItemsAssignedBaloonViewModel(rest, Properties.Resources.AS_NewItemsAssigned));

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
        ///     Первый шаг - подключение к TFS
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

            return WindowManager.ShowDialog(FirstConnectionViewModel, Properties.Resources.AS_FirstConnection, 400, 200);
        }

        /// <summary>
        ///     Таск, с которого списываем время. Строго not null!
        /// </summary>
        /// <returns></returns>
        private WorkItem GetTask()
        {
            _currentTask?.Item?.SyncToLatest();

            if (_currentTask == null || !IsTaskAvailable(_currentTask))
            {
                var strategy = Settings.Settings.Read().Strategy;

                // Вызов по событию происходит из другого потока
                _currentTask = _safeExecutor.ExecuteInGuiThread(() => FindAvailableTask(strategy)).Result;
            }

            return _currentTask;
        }

        private WorkItemVm FindAvailableTask(WroteOffStrategy strategy)
        {
            // Тут уже запрашиваем все таски
            var vm = new ChooseTaskViewModel(_apiObserve);

            switch (strategy)
            {
                case WroteOffStrategy.Random:
                    var tasks = vm
                        .Searcher
                        .Items
                        .Where(x => IsTaskAvailable(x))
                        .ToList();

                    if (tasks.Any())
                    {
                        var random = new Random().Next(tasks.Count);
                        return tasks[random];
                    }

                    // Нет доступных тасков, надо выбрать самому
                    return FindAvailableTask(WroteOffStrategy.Watch);

                case WroteOffStrategy.Watch:

                    if (WindowManager.ShowDialog(vm, Properties.Resources.AS_ChooseWriteoffTask, 400, 200) == true)
                        return vm.Searcher.Selected;

                    // Выбрать нужно обязательно
                    return FindAvailableTask(strategy);


                default:
                    throw new Exception("Unknown strategy");
            }
        }

        private bool IsTaskAvailable(WorkItem item)
        {
            if (!item.IsTaskAvailable())
            {
                Trace.WriteLine($"{nameof(MainViewModel)}: Task {item?.Id} is not exist or has been closed");
                return false;
            }

            if (!_apiObserve.IsAssignedToMe(item))
                Trace.WriteLine($"{nameof(MainViewModel)}: Task is not assigned to me");

            return true;
        }

        private void RefreshStats()
        {
            StatsViewModel.Refresh(ApiObservable);

            var all = ApiObservable
                .GetMyWorkItems();

            CodeResponsesViewModel = new NewResponsesBaloonViewModel(
                all.Where(x => x.IsTypeOf(WorkItemTypes.ReviewResponse)).ToList(),
                all.Where(x => x.IsTypeOf(WorkItemTypes.CodeReview)).ToList(),
                ApiObservable);

            using (var settings = Settings.Settings.Read())
            {
                settings.MyWorkItems = new ObservableCollection<int>(all.Select(x => x.Id));
            }
        }

        #endregion

        #region Actions

        private bool TryStartWorkDay()
        {
            var result = false;

            using (var settings = Settings.Settings.Read())
            {
                var work = settings.CompletedWork;
                work.SyncCheckins(_apiObserve);

                RefreshStats();

                if (!settings.Begin.IsToday())
                {
                    // Что-то не зачекинили с утра
                    if (work.ScheduledTime() != 0) work.CheckinScheduledWork(_apiObserve, settings.Capacity);

                    // Выставлили сколько надо списать часов сегодня
                    settings.Capacity = _apiObserve.GetCapacity();

                    settings.Begin = DateTime.Now;

                    Trace.WriteLine($"{settings.Begin.ToShortTimeString()}: Welcome to a new day!");

                    result = true;
                }

                work.ClearPrevRecords();
            }

            return result;
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
            var result = false;

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