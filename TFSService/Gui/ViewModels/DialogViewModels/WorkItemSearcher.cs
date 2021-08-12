using System.Collections.Generic;
using System.Linq;
using Gui.Helper;
using Gui.ViewModels.Filter;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    /// <summary>
    ///     Выполняем поиск по рабочим элементам
    /// </summary>
    public class WorkItemSearcher : BindableExtended
    {
        /// <summary>
        ///     Заполняем выпадающий список элементами, привязанными на меня
        /// </summary>
        /// <param name="api"></param>
        /// <param name="types">Типы элементов, который хочу вывести. Cм. <see cref="WorkItemTypes" /></param>
        public WorkItemSearcher(IWorkItem api, IEnumerable<ItemTypeMark> types, IEnumerable<ItemTypeMark> states = null)
            : this(api, new FilterViewModel(
                new CategoryFilterViewModel(
                    Properties.Resources.AS_Filter_WorkTypes,
                    types,
                    true,
                    false,
                    true),
                new CategoryFilterViewModel(Properties.Resources.AS_Filter_WorkItemStates,
                    states,
                    true,
                    false,
                    true),
                new IgnoredItemsFilterViewModel(
                    Properties.Resources.AS_Filter_IgnoredIds,
                    new ExtendedItemTypeMark[0],
                    true)))
        {
        }

        public WorkItemSearcher(IWorkItem api, FilterViewModel filter)
        {
            _api = api;
            _action = new TimedAction<string, IList<WorkItemVm>>(PerformSearch);
            _action.Performed += OnResult;

            _items = new List<WorkItemVm>();

            Filter = new FilterViewModel(filter);
            Filter.FilterChanged += (sender, args) => UpdateByFilter(true);

            var mine = _api
                .GetMyWorkItems();

            _originItems.AddRange(mine.Select(x => new WorkItemVm(x)));
            UpdateByFilter();
        }

        #region Fields

        private readonly IWorkItem _api;
        private readonly TimedAction<string, IList<WorkItemVm>> _action;

        private WorkItemVm _selected;
        private readonly List<WorkItemVm> _originItems = new List<WorkItemVm>();
        private IList<WorkItemVm> _items;
        private string _text;
        private string _help;

        /// <summary>
        ///     Разрешенные типы рабочих элементов
        /// </summary>
        // private readonly string[] _types;

        #endregion

        #region Propeties

        public string Help
        {
            get => _help;
            set => SetProperty(ref _help, value);
        }

        public string Text
        {
            get => _text;
            set
            {
                if (SetProperty(ref _text, value)) TryScheduleSearch(Text);
            }
        }

        public WorkItemVm Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        public IList<WorkItemVm> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public FilterViewModel Filter { get; set; }

        #endregion

        #region Private Methods

        private void TryScheduleSearch(string text)
        {
            // Пустая строчка
            if (string.IsNullOrWhiteSpace(text))
                return;

            // Такой элемент уже есть, искать не надо
            if (int.TryParse(text, out var id) && Items.Any(x => x.Item.Id == id))
                return;

            _action.Shedule(text);
        }

        private IList<WorkItemVm> PerformSearch(string arg)
        {
            var list = new List<WorkItemVm>();

            // Сначала ищем по ID
            if (int.TryParse(arg, out var id)
                && _api.FindById(id) is WorkItem find)
            {
                list.Add(find);
            }
            // Не получилось - ищем по строке
            else
            {
                var finded = _api.Search(arg, Filter.WorkTypes.GetSelected());
                foreach (var item in finded) list.Add(item);
            }

            return list;
        }

        private void OnResult(object sender, IList<WorkItemVm> e)
        {
            _originItems.Clear();
            _originItems.AddRange(e);

            UpdateByFilter();
        }

        /// <summary>
        ///     Обновяем данные по фильтру
        /// </summary>
        /// <param name="shouldWebRequest">Нужно ли делать Web запрос или фильтруем что есть</param>
        private void UpdateByFilter(bool shouldWebRequest = false)
        {
            if (shouldWebRequest && !string.IsNullOrWhiteSpace(Text))
            {
                _action.Shedule(Text);
            }
            else
            {
                Items = _originItems
                    .Where(x => Filter.Accepted(x.Item))
                    .ToList();
            }
        }

        #endregion
    }
}