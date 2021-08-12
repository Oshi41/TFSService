using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.Win32;
using TfsAPI.Comparers;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;
using TfsAPI.Logger;
using TfsAPI.ObservingItems;
using TfsAPI.RulesNew;
using TfsAPI.TFS.Build_Defenitions;

namespace TfsAPI.TFS
{
    public class TfsObservable : ITFsObservable
    {
        /// <param name="url">Адрес сервера TFS</param>
        /// <param name="myItems">Список ID моих рабочих элементов</param>
        /// <param name="builds">Список сегодняшних билдов</param>
        /// <param name="coolDown">Время между обновлениями рабочих элементов в минутах</param>
        /// <param name="currentItem">Функция получения рабочего элемента для списания времени</param>
        /// <param name="rules">Функция для получения списка правил проверка рабочих элементов</param>
        /// <param name="history">Элементы, за которыми я наблюдаю</param>
        public TfsObservable(IList<IObservingItem> myItems,
            IList<string> builds,
            int coolDown,
            Func<WorkItem> currentItem,
            Func<IList<IRule>> rules,
            Func<IList<IObservingItem>> history, 
            IConnect connectService, 
            IWorkItem workItemService)
        {
            _currentItem = currentItem;
            _rules = rules;
            _history = history;
            _connectService = connectService;
            _workItemService = workItemService;

            _versionControl = _connectService.VersionControlServer;

            _resumeTimer.AddAction(TimeSpan.FromMinutes(coolDown), RequestUpdate);
            _resumeTimer.AddAction(TimeSpan.FromHours(1),
                () =>
                {
                    // Списываю часы
                    RaiseWriteOffEvent();

                    // обновляю рабочие элементы
                    RequestUpdate();
                });

            SystemEvents.SessionSwitch += OnSessionSwitched;
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => Logoff?.Invoke(this, e);

            MyItems = new List<WorkItem>(_workItemService.FindById(myItems.Select(x => x.Id)).Values);

            _cache = new MemoryCache(new MemoryCacheOptions());
            _capacitySearcher = new CachedCapacitySearcher(_cache, _connectService.Tfs);

            _buildClient = _connectService.Tfs.GetClient<BuildHttpClient>();

            _myBuilds.AddRange(builds);
            _ruleBuilder = new RuleBuilder();
        }

        private IEnumerable<WorkItem> MyItems
        {
            get => _myItems;
            set
            {
                if (Equals(MyItems, value))
                    return;

                _myItems = value.ToList();
            }
        }

        #region Fields

        private readonly ResumeTimer.ResumeTimer _resumeTimer = new ResumeTimer.ResumeTimer();

        private readonly VersionControlServer _versionControl;
        private readonly Func<WorkItem> _currentItem;
        private readonly Func<IList<IRule>> _rules;
        private readonly Func<IList<IObservingItem>> _history;

        private readonly IEqualityComparer<WorkItem> _idComparer = new IdWorkItemComparer();
        private readonly IEqualityComparer<WorkItem> _itemChangedComparer = new PartialWorkItemComparer();

        private bool _paused = true;
        private List<WorkItem> _myItems = new List<WorkItem>();
        private readonly List<string> _myBuilds = new List<string>();

        private readonly MemoryCache _cache;
        private readonly ICapacitySearcher _capacitySearcher;
        private readonly BuildHttpClient _buildClient;
        private readonly IRuleBuilder _ruleBuilder;
        private readonly IConnect _connectService;
        private readonly IWorkItem _workItemService;

        #endregion

        #region ITFsObservable

        public event EventHandler<CommitCheckinEventArgs> Checkin;
        public event EventHandler<List<WorkItem>> NewItems;
        public event EventHandler<List<WorkItem>> ItemsChanged;
        public event EventHandler<ScheduleWorkArgs> WriteOff;
        public event EventHandler Logoff;
        public event EventHandler Logon;
        public event EventHandler<IList<Build>> Builded;
        public event EventHandler<Dictionary<IRule, IList<WorkItem>>> RuleMismatch;

        public void Start()
        {
            // Уже работаем
            if (!_paused)
                return;

            _paused = false;
            RequestUpdate();

            _versionControl.CommitCheckin += OnCheckIn;
            _resumeTimer.Start();
        }

        public void Pause()
        {
            // Уже остановлены
            if (_paused)
                return;

            _paused = true;

            _versionControl.CommitCheckin -= OnCheckIn;
            _resumeTimer.Pause();
        }

        public void RequestUpdate()
        {
            // Обновляю рабочие элемениты
            UpdateWorkItems();

            // Обновляю список моих билдов
            UpdateBuilds();

            // Проверяю правила
            CheckRules();

            // Проверяю ЗНАЧИМЫЕ изменения раб. элементов
            // Обязательно это делать последним!
            GetNewHistory(_history?.Invoke(), _myItems);
        }

        #endregion

        #region Event handlers

        private void OnCheckIn(object sender, CommitCheckinEventArgs e)
        {
            LoggerHelper.WriteLine($"Changeset {e.ChangesetId} was made");

            Checkin?.Invoke(this, e);
        }

        private void OnSessionSwitched(object sender, SessionSwitchEventArgs e)
        {
            LoggerHelper.WriteLine($"Session switched to {e.Reason}");

            switch (e.Reason)
            {
                case SessionSwitchReason.ConsoleConnect:
                case SessionSwitchReason.RemoteConnect:
                case SessionSwitchReason.SessionRemoteControl:
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.SessionUnlock:
                    Logon?.Invoke(this, e);
                    break;

                default:
                    Logoff?.Invoke(this, e);
                    break;
            }
        }

        #endregion

        #region Override

        private const string MyWorkItemsKey = "myItemsCachePolicyName";

        // /// <summary>
        // ///     Добавил хэширование, чтобы не делать запросы чаще, чем раз в минуту
        // /// </summary>
        // /// <returns></returns>
        // public override WorkItemCollection GetMyWorkItems()
        // {
        //     if (!_cache.TryGetValue<WorkItemCollection>(MyWorkItemsKey, out var result))
        //     {
        //         result = base.GetMyWorkItems();
        //
        //         var options = new MemoryCacheEntryOptions()
        //             .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
        //
        //         lock (MyWorkItemsKey)
        //         {
        //             _cache.Set(MyWorkItemsKey, result, options);
        //         }
        //     }
        //
        //     return result;
        // }
        //
        // public override List<TeamCapacity> GetCapacity(DateTime start, DateTime end)
        // {
        //     return _capacitySearcher.SearchCapacities(Name, start, end);
        // }

        #endregion

        #region Updatings

        private void UpdateWorkItems()
        {
            var current = _workItemService.GetMyWorkItems();

            var added = current.Except(MyItems, _idComparer).ToList();
            var removed = MyItems.Except(current, _idComparer).ToList();

            TryRaiseItemsAdded(added);
            _myItems.AddRange(added);

            TryRaiseItemsRemoved(removed);
            removed.ForEach(x => _myItems.Remove(x));

            // Обновляю нетронутые элементы по данным из TFS
            UpdateAndSetItems(current.ToList());
        }

        /// <summary>
        ///     Запускаем событие списания часа
        /// </summary>
        private void RaiseWriteOffEvent()
        {
            // Если передан нулёвый элемент, считаем, что списывать не надо

            var item = _currentItem();
            if (item != null) WriteOff?.Invoke(this, new ScheduleWorkArgs(item, 1));
        }

        /// <summary>
        ///     Пытаемся вызвать собтие на добавление новых рабочих элементов
        /// </summary>
        /// <param name="added"></param>
        private void TryRaiseItemsAdded(IEnumerable<WorkItem> added)
        {
            if (!added.IsNullOrEmpty())
                NewItems?.Invoke(this, added.ToList());
        }

        /// <summary>
        ///     Вызываем событие для удалённых элементов
        /// </summary>
        /// <param name="removed"></param>
        private void TryRaiseItemsRemoved(IEnumerable<WorkItem> removed)
        {
            if (!removed.IsNullOrEmpty())
            {
                //TODO make smth
            }
        }

        /// <summary>
        ///     Синхронизирую данные рабочих элементов
        /// </summary>
        /// <param name="copy">Список полученных из TFS элементов</param>
        private void UpdateAndSetItems(IList<WorkItem> copy)
        {
            // Нахожу ID элементов с различиями
            var changedIds = MyItems.Except(copy, _itemChangedComparer).Select(x => x.Id).ToList();

            // Если есть изменения
            if (changedIds.Any())
            {
                // берём сразу с новыми данными
                var updated = copy.Where(x => changedIds.Contains(x.Id)).ToList();
                ItemsChanged?.Invoke(this, updated);
            }

            // записываем новые данные
            MyItems = copy;
        }

        /// <summary>
        ///     Обновляю список билдов
        /// </summary>
        private void UpdateBuilds()
        {
            // Инициализировал поиск билдов
            var buildSearcher = new BuildSearcher(_buildClient,
                _connectService.WorkItemStore
                    .Projects
                    .OfType<Project>()
                    .Select(x => x.Guid)
                    .ToArray());

            // нашел мои билды 
            var builds = buildSearcher
                .FindCompletedBuilds(DateTime.Today, DateTime.Now, actor: _connectService.Name)
                .ToDictionary(x => x.BuildNumber);

            // Нашел ID новых билдов
            var newBuildIds = builds.Keys.Except(_myBuilds);

            // Выцепил их значения
            var newComplited = builds.Where(x => newBuildIds.Contains(x.Key)).Select(x => x.Value).ToList();

            // Если есть хоть один, пускаю событие
            if (newComplited.Any()) Builded?.Invoke(this, newComplited);

            // Добавил их в список
            _myBuilds.AddRange(newBuildIds);
        }

        /// <summary>
        ///     Проверяю рабочие элементы на заданные правила
        /// </summary>
        private void CheckRules()
        {
            var effectiveRules = _rules?.Invoke();

            if (effectiveRules.IsNullOrEmpty())
                return;

            var inconsistent = _ruleBuilder.CheckInconsistant(effectiveRules, _workItemService);
            if (inconsistent.IsNullOrEmpty())
                return;

            RuleMismatch?.Invoke(this, inconsistent);
        }

        /// <summary>
        /// Обновляю историю рабочих элементов
        /// </summary>
        /// <param name="history">Предыдущая история рабочих эементов</param>
        /// <param name="current">История рабочих элементов, требущая обновления</param>
        /// <returns></returns>
        private void GetNewHistory(IEnumerable<IObservingItem> history, IList<WorkItem> current)
        {
            if (history.IsNullOrEmpty())
            {
                new List<IObservingItem>();
                return;
            }

            // Выцепил те ID, которые уже актуализированы
            var currentIds = current.Select(x => x.Id).ToList();

            // Вытащил те Id, которые необходимо запросить
            var needToRequest = history.Select(x => x.Id).Except(currentIds).ToList();

            // Запрос к TFS
            var finded = _workItemService.FindById(needToRequest);

            // Вписал в мапу акутальные значения
            foreach (var item in current)
            {
                finded[item.Id] = item;
            }

            // Актуальная история элементов
            var result = finded.Select(x => (IObservingItem)new ObservingItemJson(x.Value)).ToList();

            // Нашёл отличия
            var changed = history.Except(result).ToList();

            // Выбрасываю событие, если было изменение
            if (changed.Any())
            {
                // Вытаскиваю рабочие элементы для событий
                var changedWorkItems = changed
                    .Where(x => finded.ContainsKey(x.Id))
                    .Select(x => finded[x.Id])
                    .ToList();

                ItemsChanged?.Invoke(this, changedWorkItems);
            }
        }

        #endregion
    }
}