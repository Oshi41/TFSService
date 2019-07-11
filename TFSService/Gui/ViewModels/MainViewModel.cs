using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Gui.Helper;
using Gui.Properties;
using Gui.Settings;
using Gui.ViewModels.DialogViewModels;
using Gui.ViewModels.Notifications;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;
using TfsAPI.RulesNew;
using TfsAPI.TFS;
using System.DirectoryServices.AccountManagement;

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
            WriteOffHorsCommand = new ObservableCommand(OnWriteHours);

            Init();
        }

        private async void Init()
        {
            IsBusy = true;

            // Пропускаю первое вхождение (инициализация)
            _itemsChangedArbiter.Skip(1);

            var connect = await TryConnect() == true;

            if (!connect)
            {
                Trace.WriteLine($"{nameof(MainViewModel)}.{nameof(Init)}: User denied to connect, exit the program");
                Application.Current.Shutdown(0);
                return;
            }

            using (var settings = Settings.Settings.Read())
            {
                ApiObservable = await Task.Run(() => new TfsObservable(FirstConnectionViewModel.Text,
                    settings.MyWorkItems,
                    settings.MyBuilds,
                    settings.ItemMinutesCheck,
                    GetTask,
                    () => Settings.Settings.Read().Rules));

                if (!settings.Connections.Contains(FirstConnectionViewModel.Text))
                    settings.Connections.Add(FirstConnectionViewModel.Text);
            }

            await Task.Run(() =>
            {
                TryStartWorkDay();

                // Начинаем наблюдение
                _apiObserve.Start();
            });

            IsBusy = false;
        }

        #region Fields

        private readonly ActionArbiter _itemsChangedArbiter = new ActionArbiter();
        private readonly SafeExecutor _safeExecutor;

        private ITFsObservable _apiObserve;

        private WorkItemVm _currentTask;
        private StatsViewModel _statsViewModel = new StatsViewModel();
        private bool _isBusy = true;
        private NewResponsesBaloonViewModel _codeResponsesViewModel;

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
                    ApiObservable.Builded -= OnBuilded;
                    ApiObservable.RuleMismatch -= OnRuleMismatch;
                }

                _apiObserve = value;

                if (ApiObservable != null)
                {
                    ApiObservable.WriteOff += ScheduleHour;
                    ApiObservable.Logoff += OnLogoff;
                    ApiObservable.Logon += OnLogon;
                    ApiObservable.NewItems += OnNewItems;
                    ApiObservable.ItemsChanged += OnItemsChanged;
                    ApiObservable.Builded += OnBuilded;
                    ApiObservable.RuleMismatch += OnRuleMismatch;
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
            get => _statsViewModel;
            set => SetProperty(ref _statsViewModel, value);
        }

        public NewResponsesBaloonViewModel CodeResponsesViewModel
        {
            get => _codeResponsesViewModel;
            set => SetProperty(ref _codeResponsesViewModel, value);
        }

        /// <summary>
        ///     Закрыть ли окошко шторкой
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
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

        /// <summary>
        ///     Принудительное обновление
        /// </summary>
        public ICommand UpdateCommand { get; }

        /// <summary>
        ///     Открыть настройки
        /// </summary>
        public ICommand SettingsCommand { get; }

        /// <summary>
        /// Принудительно списываю время
        /// </summary>
        public ICommand WriteOffHorsCommand { get; }

        #endregion

        #region Command handler

        private void ShowMonthly()
        {
            WindowManager.ShowDialog(new MonthCheckinsViewModel(_apiObserve), Resources.AS_MonthlySchedule, 700, 500);
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

            WindowManager.ShowDialog(vm, Resources.AS_Settings, 500);
        }

        private void OnWriteHours()
        {
            var vm = new WriteOffHoursViewModel(_apiObserve);

            if (WindowManager.ShowDialog(vm, Resources.AS_ChooseWriteoffTask, 450, 240) == true)
            {
                var selected = vm.ChooseTaskVm.Searcher.Selected;
                _apiObserve.WriteHours(selected, (byte) vm.Hours, true);
            }
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
            WriteSession();

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

                if (bugs.Any()) WindowManager.ShowBaloon(new ItemsAssignedBaloonViewModel(bugs, Resources.AS_NewBugs));

                if (works.Any())
                    WindowManager.ShowBaloon(new ItemsAssignedBaloonViewModel(works, Resources.AS_NewWork));

                if (rare.Any())
                    WindowManager.ShowBaloon(new ItemsAssignedBaloonViewModel(rare, Resources.AS_ImportantNotice));

                if (responses.Any())
                {
                    var vms = new NewResponsesBaloonViewModel(responses, requests, _apiObserve,
                        Resources.AS_CodeReviewRequested);
                }

                if (rest.Any())
                    WindowManager.ShowBaloon(
                        new ItemsAssignedBaloonViewModel(rest, Resources.AS_NewItemsAssigned));

                RefreshStats();
            }
        }

        /// <summary>
        ///     Обновлениями рабочих элементов управляются специальным арбитром
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemsChanged(object sender, Dictionary<WorkItem, List<WorkItemEventArgs>> e)
        {
            _itemsChangedArbiter.Do(() =>
            {
                if (!e.IsNullOrEmpty())
                {
                    RefreshStats();

                    // Доступные элементы
                    var active = e.Where(x => !x.Key.HasState(WorkItemStates.Closed)).Select(x => x.Key).ToList();

                    if (active.Any())
                    {
                        var baloon = new ItemsAssignedBaloonViewModel(active, Resources.AS_ItemsChanged);
                        WindowManager.ShowBaloon(baloon);
                    }
                }
            });
        }

        private void OnBuilded(object sender, IList<Build> e)
        {
            using (var settings = Settings.Settings.Read())
            {
                var savedIds = settings.MyBuilds;
                var newIds = e.Select(x => x.BuildNumber).ToList();

                var brandNewIds = newIds.Except(savedIds).ToList();

                if (!brandNewIds.Any())
                    return;

                var brandNewBuild = e
                    .Where(x => savedIds
                        .Contains(x.BuildNumber))
                    .ToList();

                // Сохранили в настройки
                settings.MyBuilds = new ObservableCollection<string>(brandNewIds.Concat(savedIds));

                foreach (var build in brandNewBuild)
                    if (build.Result == BuildResult.Succeeded)
                        WindowManager.ShowBalloonSuccess(string.Format(Resources.AS_StrFormat_BuildedSuccecfully,
                            build.BuildNumber));
                    else
                        WindowManager
                            .ShowBalloonError(
                                string.Format(Resources.AS_StrFormat_BuildedWithError,
                                    build.BuildNumber,
                                    build.Result));
            }
        }

        private void OnRuleMismatch(object sender, Dictionary<IRule, IList<WorkItem>> e)
        {
            if (e.IsNullOrEmpty())
                return;

            // Прохожу по всем правилам, где есть ненулевой список неподходящих рабочих элементов
            foreach (var rule in e.Keys.Where(x => !e[x].IsNullOrEmpty()))
            {
                var vm = new ItemsAssignedBaloonViewModel(e[rule], rule.Title + "\nНесовпадения:");
                WindowManager.ShowBaloon(vm);
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

            // Однозначно значем куда подключаться
            if (FirstConnectionViewModel.RememberedConnections.Count == 1
                && FirstConnectionViewModel.CanConnect())
            {
                await FirstConnectionViewModel.Connect();
                return FirstConnectionViewModel.IsConnected;
            }

            return WindowManager.ShowDialog(FirstConnectionViewModel, Resources.AS_FirstConnection, 400, 200);
        }

        /// <summary>
        ///     Таск, с которого списываем время. Строго not null!
        /// </summary>
        /// <returns></returns>
        private WorkItem GetTask()
        {
            lock (_apiObserve)
            {
                _currentTask?.Item?.SyncToLatest();

                if (_currentTask == null || !IsTaskAvailable(_currentTask))
                {
                    var strategy = Settings.Settings.Read().Strategy;

                    // Вызов по событию происходит из другого потока
                    _currentTask = _safeExecutor.ExecuteInGuiThread(() => FindAvailableTask(strategy)).Result;
                }
            }

            return _currentTask;
        }

        /// <summary>
        ///     В зависимости от стратегии выбираем рабочий элемент
        /// </summary>
        /// <param name="strategy"></param>
        /// <returns></returns>
        private WorkItemVm FindAvailableTask(WroteOffStrategy strategy)
        {
            // Тут уже запрашиваем все таски на мне
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
                        // Выбираю случайный элемент
                        var random = new Random().Next(tasks.Count);
                        return tasks[random];
                    }

                    // Нет доступных тасков, надо выбрать самому
                    return FindAvailableTask(WroteOffStrategy.Watch);

                case WroteOffStrategy.Watch:

                    // Только одно окошко
                    if (WindowManager.ShowDialog(vm, Resources.AS_ChooseWriteoffTask, 400, 200) == true)
                        return vm.Searcher.Selected;

                    // Выбрать нужно обязательно
                    return FindAvailableTask(strategy);


                default:
                    throw new Exception("Unknown strategy");
            }
        }

        /// <summary>
        ///     Доступен ли таск для списания часов
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool IsTaskAvailable(WorkItem item)
        {
            // Рабочий элемент должен быть не закрытым таском 
            if (!item.IsTaskAvailable())
            {
                Trace.WriteLine(
                    $"{nameof(MainViewModel)}.{nameof(IsTaskAvailable)}: Task {item?.Id} is not exist or has been closed");
                return false;
            }

            // У него должно быть запланированные часы работы
            if (!(item.Fields[WorkItems.Fields.Remaining]?.Value is double remaining) || remaining <= 0)
            {
                Trace.WriteLine(
                    $"{nameof(MainViewModel)}.{nameof(IsTaskAvailable)}: Task {item?.Id} is not exist or remaining time is over");
                return false;
            }

            // Таск должен быть на мне
            if (!_apiObserve.IsAssignedToMe(item))
                Trace.WriteLine($"{nameof(MainViewModel)}.{nameof(IsTaskAvailable)}: Task is not assigned to me");

            return true;
        }

        /// <summary>
        ///     Обновляю отображение по данным из TFS
        /// </summary>
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

        private void WriteSession()
        {
            using (var settings = Settings.Settings.Read())
            {
                var user = UserPrincipal.Current;
                var logoff = DateTime.Now;
                var logon = user.LastLogon;

                if (logon.HasValue)
                {
                    settings.DisplayTime.AddDate(logon.Value, true);
                }

                settings.DisplayTime.AddDate(logoff, false);
            }
        }

        #endregion

        #region Actions

        /// <summary>
        ///     Пытаюсь начать рабочий день. True, если получилось
        /// </summary>
        /// <returns></returns>
        private bool TryStartWorkDay()
        {
            var result = false;

            using (var settings = Settings.Settings.Read())
            {
                // Получил запланированную вчера работу и пытаюсь зачекинть.
                var work = settings.CompletedWork;
                work.SyncCheckins(_apiObserve);

                if (!settings.DisplayTime.GetBegin().IsToday())
                {
                    //Начинаю рабочий день!
                    Trace.WriteLine($"{nameof(Settings)}.{nameof(TryStartWorkDay)}: " +
                        $"{settings.DisplayTime.GetDisplayTime().TotalHours}h: " +
                        $"{settings.DisplayTime.GetDisplayTime().Minutes}m:");

                    // Очищаю день
                    settings.DisplayTime.ClearPreviouse();

                    // Что-то не зачекинили с утра
                    if (work.ScheduledTime() != 0) work.CheckinScheduledWork(_apiObserve, settings.Capacity.Hours);

                    // Если выставили трудозатраты не сами, то получаем из TFS
                    if (!settings.Capacity.ByUser) settings.Capacity.Hours = _apiObserve.GetCapacity();

                    settings.DisplayTime.AddDate(DateTime.Now, true);

                    Trace.WriteLine(
                        $"{nameof(Settings)}.{nameof(TryStartWorkDay)}: {settings.DisplayTime.GetBegin().ToShortTimeString()}: Welcome to a new day!");

                    result = true;
                }

                // Очищаю вчерашние рабочие элементы
                work.ClearPrevRecords();
                // очищаю список моих билдов
                settings.MyBuilds.Clear();
            }

            RefreshStats();

            return result;
        }

        /// <summary>
        ///     Прошел час, списываю его и помещаю в конфиг
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hours"></param>
        private void ScheduleHour(int id, byte hours)
        {
            using (var settigs = Settings.Settings.Read())
            {
                settigs.CompletedWork.ScheduleWork(id, hours);

                // Сразу же списываем час работы
                settigs.CompletedWork.CheckinScheduledWork(_apiObserve, settigs.Capacity.Hours);

                RefreshStats();
            }
        }

        /// <summary>
        ///     Пытаюсь завершить рабочий день
        /// </summary>
        /// <returns></returns>
        private bool TryEndWorkDay()
        {
            var result = false;

            using (var settings = Settings.Settings.Read())
            {
                var now = DateTime.Now;
                var work = settings.CompletedWork;

                // 1) С начала наблюдения уже прошло нужное кол-во часов
                if (now - settings.DisplayTime.GetBegin() > TimeSpan.FromHours(settings.Capacity.Hours))
                {
                    work.SyncDailyPlan(_apiObserve, settings.Capacity.Hours, GetTask);
                    result = true;
                }
                else
                {
                    // 2) Пользователь уже распланировал достаточно времени
                    if (settings.Capacity.Hours <= work.ScheduledTime() + work.CheckinedTime())
                    {
                        work.CheckinScheduledWork(_apiObserve, settings.Capacity.Hours);
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