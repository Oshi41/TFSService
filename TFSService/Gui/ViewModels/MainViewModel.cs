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
using TfsAPI.Extentions;
using TfsAPI.Interfaces;
using TfsAPI.TFS;

namespace Gui.ViewModels
{
    public class MainViewModel
    {
        #region Fields

        private ITFsObservable _apiObserve;

        private WorkItemVm _currentTask;

        #endregion

        #region Properties

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
        }

        #region Methods

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
                _apiObserve = new TfsObservable(FirstConnectionViewModel.Text, settings.MyWorkItems, GetTask);
            }
        }

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

        #region Actions

        private void StartWorkDay()
        {

        }

        #endregion

        #endregion

    }
}
