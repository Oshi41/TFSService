using System.Linq;
using Gui.Helper;
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
                Help = Properties.Resources.AS_ChooseTask
            };
        }

        public WorkItemSearcher Searcher
        {
            get => _searcher;
            set => SetProperty(ref _searcher, value);
        }

        private void CreateTask()
        {
            var vm = new CreateTaskViewModel(_tfs);

            if (WindowManager.ShowDialog(vm, Properties.Resources.AS_TaskCreation, 500) == true)
            {
                var copy = Searcher.Items.ToList();
                copy.Add(vm.CreatedItem);

                Searcher.Items = copy;

                // И сразу ставим его в селект
                Searcher.Selected = vm.CreatedItem;
            }
        }

        protected override string ValidateProperty(string prop)
        {
            if (prop == nameof(Searcher.Selected)
                && !Searcher.Selected.Item.IsTypeOf(WorkItemTypes.Task))
                return Properties.Resources.AS_NotATask_Error;

            return base.ValidateProperty(prop);
        }
    }
}