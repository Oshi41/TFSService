using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gui.Settings;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using Mvvm;
using Mvvm.Commands;
using Newtonsoft.Json;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.Filter
{
    public class IgnoredItemsFilterViewModel : BindableBase, ICategoryFilterViewModel, IInitializable
    {
        private bool _isEnable;
        private string _currentId;
        private ITfsApi _api;
        private readonly List<WorkItemVm> _all = new List<WorkItemVm>();

        private ObservableCollection<WorkItemVm> _myWorkItems;

        private WorkItemVm _selected;
        //private List<WorkItemVm> _all = new List<WorkItemVm>();
        //private ObservableCollection<WorkItemVm> _myWorkItems = new ObservableCollection<WorkItemVm>();

        //[JsonConstructor]
        //public IgnoredItemsFilterViewModel(string title, IEnumerable<ItemTypeMark> marks, bool isEnable)
        //    : this(title, marks, isEnable, true, false)
        //{
        //}

        //public IgnoredItemsFilterViewModel(string title, IEnumerable<ItemTypeMark> marks, bool isEnable,
        //    bool canDisable, bool shouldRestrictNotSelected)
        //    : base(title, marks, isEnable, canDisable, shouldRestrictNotSelected)
        //{
        //    AddMark = new DelegateCommand(OnAddMark, OnCanAddMark);
        //    DeleteMark = new DelegateCommand<ItemTypeMark>(OnDeleteMark);
        //}

        //[JsonIgnore]
        //public string CurrentId { get; set; }

        //[JsonIgnore]
        //public ObservableCollection<WorkItemVm> MyWorkItems
        //{
        //    get => _myWorkItems;
        //    set => SetProperty(ref _myWorkItems, value);
        //}

        //[JsonIgnore] public ICommand AddMark { get; }
        //[JsonIgnore] public ICommand DeleteMark { get; }

        //private void OnAddMark()
        //{
        //    var item = _all.FirstOrDefault(x => x.Item.Id.ToString().Equals(CurrentId));
        //    var mark = item == null
        //        ? new ExtendedItemTypeMark(CurrentId)
        //        : new ExtendedItemTypeMark(item);

        //    Marks.Add(mark);
        //    Initialize(_all);
        //    RaiseFitlerChanged();
        //}

        //private void OnDeleteMark(ItemTypeMark obj)
        //{
        //    Marks.Remove(obj);
        //    Initialize(_all);
        //    RaiseFitlerChanged();
        //}

        //private bool OnCanAddMark()
        //{
        //    return !string.IsNullOrEmpty(CurrentId) &&
        //           !Marks.Select(x => x.Value).Any(x => string.Equals(x, CurrentId));
        //}

        ///// <summary>
        ///// Устанавливает в комбобокс список элементов, которые на мне
        ///// </summary>
        ///// <param name="originItems"></param>
        //public void Initialize(List<WorkItemVm> originItems, ITfsApi api = null)
        //{
        //    _all = originItems.ToList();
        //    var ids = Marks.Select(x => x.Value).ToList();
        //    MyWorkItems = new ObservableCollection<WorkItemVm>(_all.Where(x => !ids.Contains(x.Item.Id.ToString())));

        //}

        [JsonConstructor]
        public IgnoredItemsFilterViewModel(string title,
            IEnumerable<ExtendedItemTypeMark> marks,
            bool isEnable)
            : this(title, marks, isEnable, true)
        {
        }

        public IgnoredItemsFilterViewModel(string title,
            IEnumerable<ExtendedItemTypeMark> marks,
            bool isEnable,
            bool canDisable)
        {
            Title = title;
            IsEnable = isEnable;
            Marks = new ObservableCollection<IItemTypeMark>(marks);
            CanDisable = canDisable;

            Marks.CollectionChanged += NotifyChanges;
            NotifyChanges(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Marks));

            AddMark = new DelegateCommand(OnAddMark, OnCanAddMark);
            DeleteMark = new DelegateCommand<IItemTypeMark>(OnDeleteMark);
        }

        #region ICategoryFilterViewModel

        public bool IsEnable
        {
            get => _isEnable;
            set
            {
                if (SetProperty(ref _isEnable, value))
                {
                    RaiseFitlerChanged();
                }
            }
        }

        public ObservableCollection<IItemTypeMark> Marks { get; }

        public string Title { get; }
        public bool CanDisable { get; }
        public event EventHandler FilterChanged;

        #endregion

        #region Properties

        [JsonIgnore]
        public string CurrentId
        {
            get => _currentId;
            set => SetProperty(ref _currentId, value);
        }

        [JsonIgnore]
        public WorkItemVm Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        [JsonIgnore] public ICommand AddMark { get; }
        [JsonIgnore] public ICommand DeleteMark { get; }

        [JsonIgnore]
        public ObservableCollection<WorkItemVm> MyWorkItems
        {
            get => _myWorkItems;
            set => SetProperty(ref _myWorkItems, value);
        }

        #endregion

        /// <summary>
        /// Устанавливает список рабочих элементов в подсказке
        /// </summary>
        /// <param name="all"></param>
        /// <param name="api"></param>
        public async void Initialize(IEnumerable<WorkItemVm> all, ITfsApi api = null)
        {
            _all.Clear();
            _all.AddRange(all);

            var source = all;

            if (api != null)
            {
                _api = api;

                var finded = await Task.Run(() => api
                    .FindById(Marks.Select(x => x.Value)
                        .Except(_all
                            .Select(x => x.Item.Id.ToString()))
                        .Select(x => new
                        {
                            IsParsed = int.TryParse(x, out var id),
                            Id = id
                        }).Where(x => x.IsParsed).Select(x => x.Id)));

                source = source.Union(finded.Values.Select(x => new WorkItemVm(x)));
            }

            Marks.OfType<IInitializable>().ForEach(x => x.Initialize(source, api));

            var ids = Marks.Select(x => x.Value).Distinct().ToList();
            MyWorkItems = new ObservableCollection<WorkItemVm>(_all.Where(x => !ids.Contains(x.Item.Id.ToString())));
        }

        #region private

        private void NotifyChanges(object sender, NotifyCollectionChangedEventArgs e)
        {
            var old = e?.OldItems?.OfType<INotifyPropertyChanged>().ToList();
            var added = e?.NewItems?.OfType<INotifyPropertyChanged>().ToList();

            if (!old.IsNullOrEmpty()) old.ForEach(x => x.PropertyChanged -= MarkDirty);

            if (!added.IsNullOrEmpty()) added.ForEach(x => x.PropertyChanged += MarkDirty);

            Initialize(_all.ToList());
        }

        private void MarkDirty(object sender, PropertyChangedEventArgs e)
        {
            RaiseFitlerChanged();
        }

        private void RaiseFitlerChanged()
        {
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool OnCanAddMark()
        {
            return !string.IsNullOrWhiteSpace(CurrentId);
        }

        private void OnAddMark()
        {
            if (Selected != null)
            {
                Marks.Add(new ExtendedItemTypeMark(Selected.Item));
                return;
            }

            if (!string.IsNullOrWhiteSpace(CurrentId))
            {
                var mark = new ExtendedItemTypeMark(CurrentId);
                mark.Initialize(_all, _api);

                if (mark.WorkItem != null)
                    Marks.Add(mark);
            }
        }

        private void OnDeleteMark(IItemTypeMark obj)
        {
            Marks.Remove(obj);
        }

        #endregion
    }

    public class ExtendedItemTypeMark : BindableBase, IItemTypeMark, IInitializable
    {
        private bool _isChecked;
        private bool _isEnabled;
        private string _value;
        private string _toolTip;
        private WorkItemVm _workItem;

        [JsonConstructor]
        public ExtendedItemTypeMark(string value, bool isChecked = true, bool isEnabled = true)
        {
            Value = value;
            IsChecked = isChecked;
            IsEnabled = isEnabled;

            CheckCommand = new DelegateCommand(() => IsChecked = !IsChecked, () => isEnabled);
        }

        public ExtendedItemTypeMark(WorkItemVm item)
            : this(item.Item.Id.ToString())
        {
            WorkItem = item;
        }

        public async void Initialize(IEnumerable<WorkItemVm> all, ITfsApi api = null)
        {
            if (!int.TryParse(Value, out var id))
                return;

            // Сначала ищу в списке, лиюо делаю запрос
            WorkItem item = all.FirstOrDefault(x => x.Item.Id == id)?.Item
                            ?? await Task.Run(() => api?.FindById(id));

            WorkItem = item;
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        [JsonIgnore]
        public string ToolTip
        {
            get => _toolTip;
            set => SetProperty(ref _toolTip, value);
        }

        [JsonIgnore]
        public WorkItemVm WorkItem
        {
            get => _workItem;
            set
            {
                if (SetProperty(ref _workItem, value) && value?.Item != null)
                {
                    Value = value.Item.Id.ToString();
                    ToolTip = value.Item.Type.Name + " " + value.Item.Title;
                }
            }
        }

        [JsonIgnore] public ICommand CheckCommand { get; }
    }
}