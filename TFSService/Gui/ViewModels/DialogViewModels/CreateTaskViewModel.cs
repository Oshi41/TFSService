using Gui.Helper;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    public class CreateTaskViewModel : BindableExtended
    {
        private readonly ITfsApi _tfs;
        private uint _hours;
        private string _title;

        public CreateTaskViewModel(ITfsApi tfs)
        {
            _tfs = tfs;
            Searcher = new WorkItemSearcher(tfs,
                WorkItemTypes.Pbi,
                WorkItemTypes.Bug, 
                WorkItemTypes.Improvement, 
                WorkItemTypes.Incident)
            {
                Help = Properties.Resources.AS_ChooseParentItem,
            };

            SubmitCommand = new ObservableCommand(CreateTask, OnCanCreateTask);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public uint Hours
        {
            get => _hours;
            set => SetProperty(ref _hours, value);
        }

        public WorkItemSearcher Searcher { get; set; }

        public WorkItem CreatedItem { get; private set; }

        private bool OnCanCreateTask()
        {
            return Searcher.Selected != null
                   && Hours > 0
                   && !string.IsNullOrWhiteSpace(Title);
        }

        private void CreateTask()
        {
            CreatedItem = _tfs.CreateTask(Title, Searcher.Selected, Hours);
        }

        protected override string ValidateProperty(string prop)
        {
            if (prop == nameof(Title) && string.IsNullOrWhiteSpace(Title)) return Properties.Resources.AS_EmptyName_Error;

            if (prop == nameof(Hours) && Hours < 1) return Properties.Resources.AS_PlannedTime_Error;

            return base.ValidateProperty(prop);
        }

        protected override string ValidateOptionalProperty(string prop)
        {
            if (prop == nameof(Hours) && Hours > 40) return Properties.Resources.AS_TooBigTask_Asking;

            return base.ValidateOptionalProperty(prop);
        }
    }
}