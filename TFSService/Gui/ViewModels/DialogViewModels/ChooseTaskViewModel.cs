using Gui.Helper;
using Mvvm.Commands;
using TfsAPI.Constants;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    class ChooseTaskViewModel : BindableExtended
    {
        private readonly ITfsApi _tfs;
        private WorkItemSearcher _searcher;

        public ChooseTaskViewModel(ITfsApi tfs)
        {
            _tfs = tfs;

            Searcher = new WorkItemSearcher(tfs, WorkItemTypes.Task, WorkItemTypes.Pbi)
            {
                Help = "Выберите рабочий элемент:"
            };

            SpecialCommand = new ObservableCommand(OnCreate);

        }

        private void OnCreate()
        {
            
        }

        public WorkItemSearcher Searcher
        {
            get => _searcher;
            set => SetProperty(ref _searcher, value);
        }
    }
}
