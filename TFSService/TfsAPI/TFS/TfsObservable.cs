using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.Win32;
using TfsAPI.Comarers;
using TfsAPI.Interfaces;
using TfsAPI.RulesNew;
using TfsAPI.TFS.Build_Defenitions;

namespace TfsAPI.TFS
{
    public class TfsObservable : TfsApi, ITFsObservable
    {
        /// <param name="url">Адрес сервера TFS</param>
        /// <param name="myItems">Список ID моих рабочих элементов</param>
        /// <param name="builds">Список сегодняшних билдов</param>
        /// <param name="coolDown">Время между обновлениями рабочих элементов в минутах</param>
        /// <param name="currentItem">Функция получения рабочего элемента для списания времени</param>
        /// <param name="rules">Функция для получения списка правил проверка рабочих элементов</param>
        public TfsObservable(string url,
            IList<int> myItems,
            IList<string> builds,
            int coolDown,
            Func<WorkItem> currentItem,
            Func<IList<IRule>> rules)
            : base(url)
        {
            _currentItem = currentItem;
            _rules = rules;
            _versionControl = Project.GetService<VersionControlServer>();
            _hourTimer = new Timer(1000 * 60 * 60);
            _hourTimer.Elapsed += (sender, args) => RequestUpdate(true);

            _itemsTimer = new Timer(1000 * 60 * coolDown);
            _hourTimer.Elapsed += (sender, args) => RequestUpdate();

            SystemEvents.SessionSwitch += OnSessionSwitched;
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => Logoff?.Invoke(this, e);

            MyItems = new List<WorkItem>(FindById(myItems).Values);

            _cache = new MemoryCache(new MemoryCacheOptions());
            _capacitySearcher = new CachedCapacitySearcher(_cache, Project);

            _buildClient = Project.GetClient<BuildHttpClient>();

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

                if (!MyItems.IsNullOrEmpty())
                    MyItems.ForEach(x => x.FieldChanged -= RememberChange);

                _myItems = value.ToList();

                if (!MyItems.IsNullOrEmpty())
                    MyItems.ForEach(x => x.FieldChanged += RememberChange);
            }
        }

        private void RequestUpdate(bool timerEllapsed)
        {
            // Находимся в режиме ожилания
            if (_paused) return;

            // По запросу таймера спишем время
            if (timerEllapsed) RaiseWriteOffEvent();

            var current = GetMyWorkItems();

            var added = current.Except(MyItems, _comparer).ToList();
            var removed = MyItems.Except(current, _comparer).ToList();

            TryRaiseItemsAdded(added);
            TryRaiseItemsRemoved(removed);

            // Удалил элементы
            _myItems = _myItems.Except(removed, _comparer).ToList();

            // Обновляю нетронутые элементы по данным из TFS
            UpdateItems();

            // Добавил элементы
            _myItems.AddRange(added);

            // Обновляю данные по билдам
            UpdateBuilds();

            // Т.к. правила проверять постоянно не нужно
            if (timerEllapsed)
            {
                // Проверяем правила
                CheckRules();
            }
        }

        #region Fields

        private readonly VersionControlServer _versionControl;
        private readonly Timer _hourTimer;
        private readonly Timer _itemsTimer;
        private readonly Func<WorkItem> _currentItem;
        private readonly Func<IList<IRule>> _rules;
        private readonly IEqualityComparer<WorkItem> _comparer = new WorkItemComparer();

        private readonly ConcurrentDictionary<WorkItem, List<WorkItemEventArgs>> _changes =
            new ConcurrentDictionary<WorkItem, List<WorkItemEventArgs>>();

        private bool _paused = true;
        private List<WorkItem> _myItems = new List<WorkItem>();
        private readonly List<string> _myBuilds = new List<string>();

        private readonly MemoryCache _cache;
        private readonly ICapacitySearcher _capacitySearcher;
        private readonly BuildHttpClient _buildClient;
        private readonly IRuleBuilder _ruleBuilder;

        #endregion

        #region ITFsObservable

        public event EventHandler<CommitCheckinEventArgs> Checkin;
        public event EventHandler<List<WorkItem>> NewItems;
        public event EventHandler<Dictionary<WorkItem, List<WorkItemEventArgs>>> ItemsChanged;
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
            _hourTimer.Start();
            _itemsTimer.Start();
        }

        public void Pause()
        {
            // Уже остановлены
            if (_paused)
                return;

            _paused = true;

            _versionControl.CommitCheckin -= OnCheckIn;
            _hourTimer.Stop();
            _itemsTimer.Stop();
        }

        public void RequestUpdate()
        {
            RequestUpdate(false);
        }

        #endregion

        #region Event handlers

        private void OnCheckIn(object sender, CommitCheckinEventArgs e)
        {
            Trace.WriteLine($"{nameof(TfsObservable)}.{nameof(OnCheckIn)}: Changeset {e.ChangesetId} was made");

            Checkin?.Invoke(this, e);
        }

        private void OnSessionSwitched(object sender, SessionSwitchEventArgs e)
        {
            Trace.WriteLine($"{nameof(TfsObservable)}.{nameof(OnSessionSwitched)}: Session switched to {e.Reason}");

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

        private void RememberChange(object sender, WorkItemEventArgs e)
        {
            if (e?.Field?.WorkItem == null)
                return;

            _changes.AddOrUpdate(e.Field.WorkItem,
                item => new List<WorkItemEventArgs> {e},
                (item, list) =>
                {
                    list.Add(e);
                    return list;
                });
        }

        #endregion

        #region Override

        private const string MyWorkItemsKey = "myItemsCachePolicyName";

        /// <summary>
        ///     Добавил хэширование, чтобы не делать запросы чаще, чем раз в минуту
        /// </summary>
        /// <returns></returns>
        public override IList<WorkItem> GetMyWorkItems()
        {
            if (!_cache.TryGetValue<IList<WorkItem>>(MyWorkItemsKey, out var result))
            {
                result = base.GetMyWorkItems();

                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));

                lock (MyWorkItemsKey)
                {
                    _cache.Set(MyWorkItemsKey, result, options);
                }
            }

            return result;
        }

        public override List<TeamCapacity> GetCapacity(DateTime start, DateTime end)
        {
            return _capacitySearcher.SearchCapacities(Name, start, end);
        }

        #endregion

        #region Updatings

        /// <summary>
        /// Запускаем событие списания часа
        /// </summary>
        private void RaiseWriteOffEvent()
        {
            WriteOff?.Invoke(this, new ScheduleWorkArgs(_currentItem(), 1));
        }

        /// <summary>
        /// Пытаемся вызвать собтие на добавление новых рабочих элементов
        /// </summary>
        /// <param name="added"></param>
        private void TryRaiseItemsAdded(IEnumerable<WorkItem> added)
        {
            if (!added.IsNullOrEmpty())
                NewItems?.Invoke(this, added.ToList());
        }

        /// <summary>
        /// Вызываем событие для удалённых элементов
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
        /// Синхронизирую данные рабочих элементов
        /// </summary>
        /// <param name="items"></param>
        private void UpdateItems()
        {
            // Каждое изменение генерит кучу событий
            // запускаем обновление для каждого раюочего элемента
            MyItems
                .AsParallel()
                .ForEach(x => x.SyncToLatest());

            // Изменений нет, выходим
            if (_changes.IsEmpty)
                return;

            // Запускаем событие
            ItemsChanged?.Invoke(this, _changes.ToDictionary(x => x.Key, x => x.Value));
            // очищаем запомненные изменения
            _changes.Clear();
        }

        /// <summary>
        /// Обновляю список билдов
        /// </summary>
        private void UpdateBuilds()
        {
            // Инициализировал поиск билдов
            var buildSearcher = new BuildSearcher(_buildClient, 
                _capacitySearcher
                    .GetAllMyTeams()
                    .Select(x => x.Identity.TeamFoundationId)
                    .ToArray());

            // нашел мои билды 
            var builds = buildSearcher
                .FindCompletedBuilds(DateTime.Today, DateTime.Now, actor:Name)
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
        /// Проверяю рабочие элементы на заданные правила
        /// </summary>
        private void CheckRules()
        {
            var effectiveRules = _rules?.Invoke();

            if (effectiveRules.IsNullOrEmpty())
                return;

            var inconsistent = _ruleBuilder.CheckInconsistant(effectiveRules, this);
            if (inconsistent.IsNullOrEmpty())
                return;

            RuleMismatch?.Invoke(this, inconsistent);
        }

        #endregion
    }
}