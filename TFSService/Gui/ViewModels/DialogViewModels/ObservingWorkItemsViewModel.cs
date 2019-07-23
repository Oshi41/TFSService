using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gui.Helper;
using Gui.Properties;
using Mvvm.Commands;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    class ObservingWorkItemsViewModel : BindableExtended
    {
        private readonly ITfsApi _api;
        private ObservableCollection<WorkItemVm> _observingItems;
        private bool _isBusy = true;

        public ObservableCollection<WorkItemVm> ObservingItems
        {
            get => _observingItems;
            set => SetProperty(ref _observingItems, value);
        }

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }

        public WorkItemSearcher ItemSearcher { get; private set; }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ObservingWorkItemsViewModel(ITfsApi api, IList<IObservingItem> items)
        {
            _api = api;

            AddCommand = new DelegateCommand(OnAdd);
            RemoveCommand = new DelegateCommand<WorkItemVm>(OnRemove, OnCanRemove);
            // Использую дефолтный фильтр
            // Инициализировать строго в этом потоке!!!
            ItemSearcher = new WorkItemSearcher(_api, new FilterViewModel(null, null));

            Init(items);
        }

        private async void Init(IList<IObservingItem> items)
        {
            
            var finded = await Task.Run(() => _api.FindById(items.Select(x => x.Id)));
            ObservingItems = new ObservableCollection<WorkItemVm>(finded.Values.Select(x => new WorkItemVm(x)));

            IsBusy = false;
        }

        private void OnRemove(WorkItemVm obj)
        {
            ObservingItems.Remove(obj);
        }

        private bool OnCanRemove(WorkItemVm arg)
        {
            return arg != null && ObservingItems.Contains(arg);
        }

        private void OnAdd()
        {
            if (WindowManager.ShowDialog(ItemSearcher, Resources.AS_ChooseTask, 450, 240) == true
                && ItemSearcher.Selected != null
                && !ObservingItems.Contains(ItemSearcher.Selected))
            {
                ObservingItems.Add(ItemSearcher.Selected);
            }
        }
    }
}
