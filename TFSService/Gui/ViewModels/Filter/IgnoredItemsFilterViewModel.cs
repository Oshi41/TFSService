using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Mvvm.Commands;
using Newtonsoft.Json;

namespace Gui.ViewModels.Filter
{
    public class IgnoredItemsFilterViewModel : CategoryFilterViewModel
    {
        private ObservableCollection<string> _myWorkItems = new ObservableCollection<string>();

        [JsonConstructor]
        public IgnoredItemsFilterViewModel(string title, IEnumerable<ItemTypeMark> marks, bool isEnable)
            : this(title, marks, isEnable, true, false)
        {
        }

        public IgnoredItemsFilterViewModel(string title, IEnumerable<ItemTypeMark> marks, bool isEnable,
            bool canDisable, bool shouldRestrictNotSelected)
            : base(title, marks, isEnable, canDisable, shouldRestrictNotSelected)
        {
            AddMark = new DelegateCommand(OnAddMark, OnCanAddMark);
            DeleteMark = new DelegateCommand<ItemTypeMark>(OnDeleteMark);
        }

        [JsonIgnore]
        public string CurrentId { get; set; }

        [JsonIgnore]
        public ObservableCollection<string> MyWorkItems
        {
            get => _myWorkItems;
            set => SetProperty(ref _myWorkItems, value);
        }

        [JsonIgnore] public ICommand AddMark { get; }
        [JsonIgnore] public ICommand DeleteMark { get; }

        private void OnAddMark()
        {
            Marks.Add(new ItemTypeMark(CurrentId));
            RaiseFitlerChanged();
        }

        private void OnDeleteMark(ItemTypeMark obj)
        {
            Marks.Remove(obj);
            RaiseFitlerChanged();
        }

        private bool OnCanAddMark()
        {
            return !string.IsNullOrEmpty(CurrentId) &&
                   !Marks.Select(x => x.Value).Any(x => string.Equals(x, CurrentId));
        }

        /// <summary>
        /// Устанавливает в комбобокс список элементов, которые на мне
        /// </summary>
        /// <param name="originItems"></param>
        public void Initialize(List<WorkItemVm> originItems)
        {
            var result = originItems.Select(x => x.Item.Id.ToString()).Distinct().ToList();
            MyWorkItems = new ObservableCollection<string>(result);
        }
    }
}