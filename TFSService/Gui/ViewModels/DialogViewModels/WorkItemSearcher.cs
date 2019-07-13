﻿using System.Collections.Generic;
using System.Linq;
using Gui.Helper;
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
        public WorkItemSearcher(ITfsApi api, params ItemTypeMark[] types)
        {
            _api = api;
            _action = new TimedAction<string, IList<WorkItemVm>>(PerformSearch);
            _action.Performed += OnResult;

            _items = new List<WorkItemVm>();

            Filter = new FilterViewModel(types?.ToArray());
            Filter.FilterChanged += (sender, args) => TryScheduleSearch(Text);

            var mine = _api.GetMyWorkItems();

            if (!types.IsNullOrEmpty())
            {
                var workTypes = types.Select(x => x.WorkType).ToArray();
                mine = mine.Where(x => x.IsTypeOf(workTypes)).ToList();
            }

            foreach (var item in mine) _items.Add(item);
        }

        #region Fields

        private readonly ITfsApi _api;
        private readonly TimedAction<string, IList<WorkItemVm>> _action;

        private WorkItemVm _selected;
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
                var types = Filter
                    ?.Marks
                    ?.Where(x => x.IsChecked)
                    .Select(x => x.WorkType)
                    ?.ToArray();

                var finded = _api.Search(arg, types);
                foreach (var item in finded) list.Add(item);
            }

            return list;
        }

        private void OnResult(object sender, IList<WorkItemVm> e)
        {
            Items = e;
        }

        #endregion
    }
}