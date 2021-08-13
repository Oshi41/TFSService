using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Gui.Helper;
using Gui.Properties;
using Gui.Settings;
using Gui.ViewModels.DialogViewModels;
using Gui.ViewModels.DialogViewModels.Trend;
using Gui.ViewModels.Filter;
using Gui.ViewModels.Notifications;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Build.WebApi.Events;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm;
using Mvvm.Commands;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;
using TfsAPI.Logger;
using TfsAPI.ObservingItems;
using TfsAPI.TFS;
using TfsAPI.TFS.Observers;

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
            WriteOffHoursCommand = new ObservableCommand(OnWriteHours);
            AddToIgnoredCommand = new DelegateCommand<WorkItemVm>(OnAddToIgnore, OnCanAddToIgnore);
            ObservableItemsCommand = new ObservableCommand(OnShowObservableItems);
            ShowTrendCommand = new ObservableCommand(OnShowTrendCommand);
            ShowBuildQueueCommand = new ObservableCommand(OnShowBuildQueue);
            ShowObserveCommand = new ObservableCommand(ShowObserverView);

            Init();

            App.Current.Exit += OnSaveSettings;
        }
        private async void Init()
        {
            IsBusy = true;

            // Пропускаю первое вхождение (инициализация)
            // _itemsChangedArbiter.Skip(1);

            var connect = await TryConnect() == true;

            if (!connect)
            {
                LoggerHelper.WriteLine($"User denied to connect, exit the program");
                Application.Current.Shutdown(0);
                return;
            }

            using (var viewSettings = new ViewSettings().Read<ViewSettings>())
            {
                StatsViewModel = new StatsViewModel(viewSettings.MainFilter);

                viewSettings.Project = FirstConnectionViewModel.ProjectName;

                if (!viewSettings.Connections.Contains(FirstConnectionViewModel.Text))
                    viewSettings.Connections.Add(FirstConnectionViewModel.Text);

                ViewMode = viewSettings.ViewMode;
            }

            _workItemService = new WorkItemService(_connectService, _connectService.Name);
            _writeOffService = new WriteOffService(_connectService, _workItemService);
            _chekinsService = new CheckinsService(_connectService, _workItemService);

            using (var qSettings = new BuildQueueSettings().Read<BuildQueueSettings>())
            {
                _buildService = new BuildService(_connectService, qSettings.QueuedBuilds, TimeSpan.FromSeconds(30));
            }

            using (var wiSettings = new WorkItemSettings().Read<WorkItemSettings>())
            {
                void OnSave()
                {
                    using var wiSettings = new WorkItemSettings().Read<WorkItemSettings>();
                    wiSettings.Items = new ObservableCollection<WorkItem>(_workItemObserver.Observed);
                }

                _workItemObserver = new WorkItemObserver(_workItemService, OnSave, wiSettings.Items, wiSettings.Delay);
                _workItemObserver.ItemsChanged += OnWorkItemsChanged;

                if (wiSettings.Observe)
                {
                    _workItemObserver.Start();
                }
            }

            using (var bldSettings = new BuildsSettings().Read<BuildsSettings>())
            {
                void OnSave()
                {
                    using var bldSettings = new BuildsSettings().Read<BuildsSettings>();
                    bldSettings.Builds = new ObservableCollection<Build>(_buildsObserver.Observed);
                }

                _buildsObserver = new BuildsObserver(_buildService, OnSave, bldSettings.Builds, bldSettings.Delay);
                _buildsObserver.BuildChanged += OnBuildChanged;

                if (bldSettings.Observe)
                {
                    _buildsObserver.Start();
                }
            }

            await Task.Run(RefreshStats);

            IsBusy = false;
        }

        #region Fields

        private readonly ActionArbiter _itemsChangedArbiter = new ActionArbiter();
        private readonly SafeExecutor _safeExecutor;
        private readonly IConnect _connectService = new ConnectService();

        private IWorkItem _workItemService;
        private IChekins _chekinsService;
        private IWriteOff _writeOffService;
        private IBuild _buildService;
        private IBuildsObserver _buildsObserver;
        private IWorkItemObserver _workItemObserver;

        private WorkItemVm _currentTask;
        private StatsViewModel _statsViewModel;
        private bool _isBusy = true;
        private NewResponsesBaloonViewModel _codeResponsesViewModel;
        public VisibleMode _viewMode;

        #endregion

        #region Properties

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

        public VisibleMode ViewMode
        {
            get => _viewMode;
            set
            {
                if (SetProperty(ref _viewMode, value))
                {
                    using (var settings = new ViewSettings().Read<ViewSettings>())
                    {
                        settings.ViewMode = ViewMode;
                    }
                }
            }
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
        /// Показываю график моих трудосгораний
        /// </summary>
        public ICommand ShowTrendCommand { get; }

        /// <summary>
        ///     Принудительное обновление
        /// </summary>
        public ICommand UpdateCommand { get; }

        /// <summary>
        ///     Открыть настройки
        /// </summary>
        public ICommand SettingsCommand { get; }

        /// <summary>
        ///     Принудительно списываю время
        /// </summary>
        public ICommand WriteOffHoursCommand { get; }

        /// <summary>
        /// Добавляю элемент в список игнорируемых
        /// </summary>
        public ICommand AddToIgnoredCommand { get; }

        /// <summary>
        /// Наблюдаемые элементы
        /// </summary>
        public ICommand ObservableItemsCommand { get; }

        public ICommand ShowBuildQueueCommand { get; }

        public ICommand ShowObserveCommand { get; }

        #endregion

        #region Command handler

        private void ShowMonthly()
        {
            WindowManager.ShowDialog(new MonthCheckinsViewModel(_writeOffService), Resources.AS_MonthlySchedule, 
                670 * 1.25,
                670);
        }

        private async Task Update()
        {
            IsBusy = true;

            RefreshStats();

            using (var settings = Settings.Settings.Read())
            {
                settings.QueuedBuilds = new ObservableCollection<Build>(_buildService.GetQueue());
            }

            IsBusy = false;
        }

        private void ShowSettings()
        {
            var vm = new SettingsViewModel(FirstConnectionViewModel.Text, _connectService);

            WindowManager.ShowDialog(vm, Resources.AS_Settings, 500);
        }

        private void OnWriteHours(object obj)
        {
            var vm = new WriteOffHoursViewModel(_workItemService);

            if (obj is WorkItemVm toWriteOff)
            {
                var find = vm.ChooseTaskVm.Searcher.Items.FirstOrDefault(x => x.Item.Id == toWriteOff.Item.Id);

                if (find != null) vm.ChooseTaskVm.Searcher.Selected = find;
            }


            if (WindowManager.ShowDialog(vm, Resources.AS_ChooseWriteoffTask, 450, 240) == true)
            {
                var selected = vm.ChooseTaskVm.Searcher.Selected;
                _writeOffService.WriteHours(selected, (byte)vm.Hours, true);
            }
        }

        private void OnShowObservableItems()
        {
            var vm = new ObservingWorkItemsViewModel(_workItemService, Settings.Settings.Read().ObservingItems);

            if (WindowManager.ShowDialog(vm, Resources.AS_ObservableItems_Title, width: 800, height: 400) == true)
            {
                using (var settings = Settings.Settings.Read())
                {
                    settings.ObservingItems =
                        new ObservableCollection<IObservingItem>(
                            vm.ObservingItems.Select(x => new ObservingItemJson(x)));
                }
            }
        }

        private void OnShowTrendCommand()
        {
            var vm = new TrendViewModel(_writeOffService, StatsViewModel.Capacity);
            WindowManager.ShowDialog(vm, Resources.AS_Trand_Title, 680, 600, maximize: true);
        }

        private void OnShowBuildQueue()
        {
            var vm = new BuildQueueViewModel(_buildService, _connectService);
            if (WindowManager.ShowDialog(vm, Resources.AS_BuidlQueue_Button, 680, 600) == true)
            {
                using (var settings = new BuildQueueSettings().Read<BuildQueueSettings>())
                {
                    settings.QueuedBuilds = new ObservableCollection<Build>(vm.OwnQueue);
                }
            }
        }
        
        private void ShowObserverView()
        {
            var vm = new ObserveViewModel();

            if (WindowManager.ShowDialog(vm, Resources.AS_BuidlQueue_Button, 400, 400 * 1.25) == true)
            {
                using (var settings = new BuildsSettings().Read<BuildsSettings>())
                {
                    settings.Delay = TimeSpan.FromSeconds(vm.BuildsDelay);
                    settings.Observe = vm.BuildsObserve;

                    if (settings.Observe)
                    {
                        _buildsObserver.Start();
                    }
                    else
                    {
                        _buildsObserver.Pause();
                    }
                }
                
                using (var settings = new WorkItemSettings().Read<WorkItemSettings>())
                {
                    settings.Delay = TimeSpan.FromSeconds(vm.WorkItemDelay);
                    settings.Observe = vm.WorkItemObserve;

                    if (settings.Observe)
                    {
                        _workItemObserver.Start();
                    }
                    else
                    {
                        _workItemObserver.Pause();
                    }
                }
            }
        }

        private void OnAddToIgnore(WorkItemVm obj)
        {
            var filter = StatsViewModel.Filter.Ignored;

            // Принудительно включаю фильтр
            if (filter.CanDisable && !filter.IsEnable)
            {
                filter.IsEnable = true;
            }

            // Нашёл элемент с тайим ID
            var finded = filter.Marks.FirstOrDefault(x => x.Value.Equals(obj.Item.Id.ToString()));
            if (finded == null)
            {
                // Либо создаю новый
                filter.Marks.Add(new ExtendedItemTypeMark(obj));
            }
            else
            {
                // Либюо включаю галочку
                finded.IsChecked = true;
            }
        }

        private bool OnCanAddToIgnore(WorkItemVm arg)
        {
            // обработка ошибок
            if (arg == null)
                return false;

            // обработка ошибок
            var filter = StatsViewModel?.Filter?.Ignored;
            if (filter == null)
                return false;

            // Можем форсированно включить фильтр
            if (filter.CanDisable && !filter.IsEnable)
                return true;

            // нашли элемент с таким ID
            var find = filter.Marks.FirstOrDefault(x => x.Value.Equals(arg.Item.Id.ToString()));
            // Либо такого нет, либ он выключен
            return find == null || find.IsChecked != true;
        }

        #endregion

        #region EventHandlers

        private void OnBuildChanged(object sender, BuildUpdatedEvent e)
        {
            if (e.Build.Result == BuildResult.Succeeded)
            {
                WindowManager.ShowBalloonSuccess(string.Format(Resources.AS_StrFormat_BuildedSuccecfully,
                    e.Build.Definition.Name));
            }
            else
            {
                if (e.Build.Status == BuildStatus.Completed)
                {
                    WindowManager
                        .ShowBalloonError(
                            string.Format(Resources.AS_StrFormat_BuildedWithError, e.Build.Definition.Name,
                                e.Build.Result));
                }
            }
        }

        private void OnWorkItemsChanged(object sender, ItemsChanged e)
        {
            var bugs = e.Items.Where(x => x.IsTypeOf(WorkItemTypes.Bug)).ToList();
            var works = e.Items.Where(x => x.IsTypeOf(WorkItemTypes.Pbi, WorkItemTypes.Improvement)).ToList();
            var rare = e.Items.Where(x => x.IsTypeOf(WorkItemTypes.Incident, WorkItemTypes.Feature)).ToList();
            var responses = e.Items.Where(x => x.IsTypeOf(WorkItemTypes.ReviewResponse)).ToList();
            var requests = e.Items.Where(x => x.IsTypeOf(WorkItemTypes.CodeReview)).ToList();
            var rest = e.Items.Except(bugs).Except(works).Except(rare).Except(responses).Except(requests).ToList();

            if (bugs.Any())
            {
                switch (e.Action)
                {
                    case CollectionChangeAction.Add:
                        WindowManager.ShowBaloon(new ItemsAssignedBaloonViewModel(bugs, Resources.AS_NewBugs));
                        break;

                    case CollectionChangeAction.Refresh:
                        break;

                    case CollectionChangeAction.Remove:
                        break;
                }
            }

            if (works.Any())
            {
                switch (e.Action)
                {
                    case CollectionChangeAction.Add:
                        WindowManager.ShowBaloon(new ItemsAssignedBaloonViewModel(works, Resources.AS_NewWork));
                        break;

                    case CollectionChangeAction.Refresh:
                        break;

                    case CollectionChangeAction.Remove:
                        break;
                }
            }


            if (rare.Any())
            {
                switch (e.Action)
                {
                    case CollectionChangeAction.Add:
                        WindowManager.ShowBaloon(new ItemsAssignedBaloonViewModel(rare, Resources.AS_ImportantNotice));
                        break;

                    case CollectionChangeAction.Refresh:
                        break;

                    case CollectionChangeAction.Remove:
                        break;
                }
            }

            if (responses.Any())
            {
                switch (e.Action)
                {
                    case CollectionChangeAction.Add:
                        var vms = new NewResponsesBaloonViewModel(responses, requests, _chekinsService,
                            Resources.AS_CodeReviewRequested);
                        break;

                    case CollectionChangeAction.Refresh:
                        break;

                    case CollectionChangeAction.Remove:
                        break;
                }
            }

            if (rest.Any())
            {
                switch (e.Action)
                {
                    case CollectionChangeAction.Add:
                        WindowManager.ShowBaloon(
                            new ItemsAssignedBaloonViewModel(rest, Resources.AS_NewItemsAssigned));
                        break;

                    case CollectionChangeAction.Refresh:
                        break;

                    case CollectionChangeAction.Remove:
                        break;
                }
            }

            RefreshStats();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Первый шаг - подключение к TFS
        /// </summary>
        /// <returns></returns>
        private async Task<bool?> TryConnect()
        {
            FirstConnectionViewModel = new FirstConnectionViewModel(_connectService);

            // Однозначно значем куда подключаться
            if (FirstConnectionViewModel.RememberedConnections.Count == 1
                && await FirstConnectionViewModel.TryConnect())
            {
                if (FirstConnectionViewModel.Connection == ConnectionType.Success)
                    return true;
            }

            return WindowManager.ShowDialog(FirstConnectionViewModel, Resources.AS_FirstConnection, 400, 300);
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
                LoggerHelper.WriteLine("Task {item?.Id} is not exist or has been closed");
                return false;
            }

            // У него должно быть запланированные часы работы
            if (!(item.Fields[WorkItems.Fields.Remaining]?.Value is double remaining) || remaining <= 0)
            {
                LoggerHelper.WriteLine("Task {item?.Id} is not exist or remaining time is over");
                return false;
            }

            // Таск должен быть на мне
            if (!_workItemService.IsAssignedToMe(item))
                LoggerHelper.WriteLine($"Task is not assigned to me");

            return true;
        }

        /// <summary>
        ///     Обновляю отображение по данным из TFS
        /// </summary>
        private void RefreshStats()
        {
            StatsViewModel.Refresh(_writeOffService, _workItemService);

            var all = _workItemService.GetMyWorkItems();

            CodeResponsesViewModel = new NewResponsesBaloonViewModel(
                all.Where(x => x.IsTypeOf(WorkItemTypes.ReviewResponse)),
                all.Where(x => x.IsTypeOf(WorkItemTypes.CodeReview)),
                _chekinsService);

            using (var settings = Settings.Settings.Read())
            {
                settings.MyWorkItems =
                    new ObservableCollection<IObservingItem>(all.Select(x => new ObservingItemJson(x)));
            }
        }

        private void OnSaveSettings(object sender, ExitEventArgs e)
        {
            using (var settings = new ViewSettings().Read<ViewSettings>())
            {
                settings.ViewMode = ViewMode;
                
                if (!settings.Connections.Contains(FirstConnectionViewModel.Text))
                {
                    settings.Connections.Add(FirstConnectionViewModel.Text);
                }

                settings.Project = FirstConnectionViewModel.ProjectName;
                settings.MainFilter = new FilterViewModel(StatsViewModel.Filter);
            }
        }
        #endregion
    }
}