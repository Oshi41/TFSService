using System.Collections.Generic;
using System.Linq;
using Gui.Helper;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm.Commands;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    public class ChooseTaskViewModel : BindableExtended
    {
        private readonly ITfsApi _tfs;
        private WorkItemSearcher _searcher;

        public ChooseTaskViewModel(ITfsApi tfs)
        {
            _tfs = tfs;
            SpecialCommand = new ObservableCommand(CreateTask);

            Searcher = new WorkItemSearcher(_tfs, WorkItemTypes.Task)
            {
                Help = "Выберите рабочий элемент:"
            };
        }

        private void CreateTask()
        {
            var vm = new CreateTaskViewModel(_tfs);

            if (WindowManager.ShowDialog(vm, "Создание рабочего элемента", 500) == true)
            {
                var copy = Searcher.Items.ToList();
                copy.Add(vm.CreatedItem);

                Searcher.Items = copy;

                // И сразу ставим его в селект
                Searcher.Selected = vm.CreatedItem;
            }
        }

        public WorkItemSearcher Searcher
        {
            get => _searcher;
            set => SetProperty(ref _searcher, value);
        }

        protected override string ValidateProperty(string prop)
        {
            if (prop == nameof(Searcher.Selected)
                && !Searcher.Selected.Item.IsTypeOf(WorkItemTypes.Task))
            {
                return "Рабочий элемент не является таском";
            }

            return base.ValidateProperty(prop);
        }
    }
}
