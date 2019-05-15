using Mvvm.Commands;
using TfsAPI.Constants;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    class ChooseTaskViewModel : BindableExtended
    {
        private readonly ITfs _tfs;
        private WorkItemSearcher _searcher;

        public ChooseTaskViewModel(ITfs tfs)
        {
            _tfs = tfs;

            Searcher = new WorkItemSearcher(tfs, WorkItemTypes.Task, WorkItemTypes.Pbi)
            {
                Help = "Выберите рабочий элемент:"
            };

            SpecialCommand = new DelegateCommand(OnCreate);

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
