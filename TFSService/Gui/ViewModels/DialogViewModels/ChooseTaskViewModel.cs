using System.Linq;
using Gui.Helper;
using Gui.Properties;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    /// <summary>
    ///     Окошко выбора рабочего элемента
    /// </summary>
    public class ChooseTaskViewModel : BindableExtended
    {
        private readonly IWorkItem _tfs;
        private WorkItemSearcher _searcher;

        public ChooseTaskViewModel(IWorkItem tfs)
        {
            _tfs = tfs;
            SpecialCommand = new ObservableCommand(CreateTask);

            Searcher = new WorkItemSearcher(_tfs,
                new ItemTypeMark[] { WorkItemTypes.Task },
                new ItemTypeMark[]
                {
                    WorkItemStates.New,
                    WorkItemStates.Active,
                })
            {
                Help = Resources.AS_ChooseTask
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

            if (WindowManager.ShowDialog(vm, Resources.AS_TaskCreation, 500, 500 / 1.5) == true)
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
                return Resources.AS_NotATask_Error;

            return base.ValidateProperty(prop);
        }
    }
}