using Gui.Helper;
using Gui.Properties;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    /// <summary>
    ///     Окошко создания нового таска
    /// </summary>
    public class CreateTaskViewModel : BindableExtended
    {
        private readonly IWorkItem _tfs;
        private uint _hours;
        private string _title;

        public CreateTaskViewModel(IWorkItem tfs)
        {
            _tfs = tfs;
            // Ищем для привязки только указанные типы
            Searcher = new WorkItemSearcher(
                _tfs,
                new ItemTypeMark[]
                {
                    WorkItemTypes.Pbi,
                    WorkItemTypes.Bug,
                    WorkItemTypes.Improvement,
                    WorkItemTypes.Incident
                },
                new ItemTypeMark[]
                {
                    WorkItemStates.Active,
                    WorkItemStates.Resolved,
                })
            {
                Help = Resources.AS_ChooseParentItem
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
            if (prop == nameof(Title) && string.IsNullOrWhiteSpace(Title)) return Resources.AS_EmptyName_Error;

            if (prop == nameof(Hours) && Hours < 1) return Resources.AS_PlannedTime_Error;

            return base.ValidateProperty(prop);
        }

        protected override string ValidateOptionalProperty(string prop)
        {
            if (prop == nameof(Hours) && Hours > 40) return Resources.AS_TooBigTask_Asking;

            return base.ValidateOptionalProperty(prop);
        }
    }
}