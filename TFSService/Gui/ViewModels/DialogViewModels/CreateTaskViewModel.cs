using Gui.Helper;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm.Commands;
using TfsAPI.Constants;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    public class CreateTaskViewModel : BindableExtended
    {
        private readonly ITfsApi _tfs;
        private string _title;
        private uint _hours;

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

        public CreateTaskViewModel(ITfsApi tfs)
        {
            _tfs = tfs;
            Searcher = new WorkItemSearcher(tfs,
                WorkItemTypes.Pbi, WorkItemTypes.Bug, WorkItemTypes.Improvement, WorkItemTypes.Incident);

            SubmitCommand = new ObservableCommand(CreateTask, OnCanCreateTask);
        }

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
            if (prop == nameof(Title) && string.IsNullOrWhiteSpace(Title))
            {
                return "Название не может быть пустым";
            }

            if (prop == nameof(Hours) && Hours < 1)
            {
                return "Необходимо указать кол-во запланированных часов на работу";
            }

            return base.ValidateProperty(prop);
        }

        protected override string ValidateOptionalProperty(string prop)
        {
            if (prop == nameof(Hours) && Hours > 40)
            {
                return "Уверен, что задачу нельзя разбить на меньшие части?";
            }

            return base.ValidateOptionalProperty(prop);
        }
    }
}
