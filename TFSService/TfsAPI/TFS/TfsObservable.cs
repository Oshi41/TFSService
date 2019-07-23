using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.Win32;
using TfsAPI.Comparers;
using TfsAPI.Extentions;
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

                _myItems = value.ToList();
            }
        }

        #region Fields

        private readonly ResumeTimer.ResumeTimer _resumeTimer = new ResumeTimer.ResumeTimer();

        private readonly VersionControlServer _versionControl;
        private readonly Func<WorkItem> _currentItem;
        private readonly Func<IList<IRule>> _rules;

        private readonly IEqualityComparer<WorkItem> _idComparer = new IdWorkItemComparer();
        private readonly IEqualityComparer<WorkItem> _itemChangedComparer = new PartialWorkItemComparer();

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

        #endregion

        #region Override

        private const string MyWorkItemsKey = "myItemsCachePolicyName";

        /// <summary>
        ///     Добавил хэширование, чтобы не делать запросы чаще, чем раз в минуту
        /// </summary>
        /// <returns></returns>
        public override WorkItemCollection GetMyWorkItems()
        {
            if (!_cache.TryGetValue<WorkItemCollection>(MyWorkItemsKey, out var result))
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

        private void UpdateWorkItems()
        {
            var current = GetMyWorkItems();

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
            // Нашли изменений
            var changed = MyItems.Except(copy, _itemChangedComparer).ToList();

            // Если есть изменения
            if (changed.Any())
                // Вызываем события
                ItemsChanged?.Invoke(this, changed.ToList());

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
                _itemStore
                    .Projects
                    .OfType<Project>()
                    .Select(x => x.Guid)
                    .ToArray());

            // нашел мои билды 
            var builds = buildSearcher
                .FindCompletedBuilds(DateTime.Today, DateTime.Now, actor: Name)
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

            var inconsistent = _ruleBuilder.CheckInconsistant(effectiveRules, this);
            if (inconsistent.IsNullOrEmpty())
                return;

            RuleMismatch?.Invoke(this, inconsistent);
        }

        #endregion
    }
}