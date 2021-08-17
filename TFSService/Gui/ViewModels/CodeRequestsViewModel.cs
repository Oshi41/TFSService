using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Windows.Input;
using Gui.Helper;
using Gui.Properties;
using Gui.ViewModels.DialogViewModels;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;
using TfsAPI.Interfaces;

namespace Gui.ViewModels
{
    public class CodeReviewViewModel
    {
        public CodeReviewViewModel(WorkItem codeRequest, IEnumerable<WorkItem> responses)
        {
            CodeRequest = codeRequest;
            var resps = responses.ToList();

            Responses = new ObservableCollection<WorkItem>(resps.Where(x => x.State == WorkItemStates.Closed));
            Waiting = new ObservableCollection<WorkItem>(resps.Where(x => x.State != WorkItemStates.Closed));

            ResponseStatuses = string.Join(", ",
                Responses.Select(x => x.Fields[WorkItems.Fields.ClosedStatus]?.Value as string)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .OrderBy(x => x));

            Reviewers = string.Join(", ", Waiting.Select(x => x.Fields[CoreField.AssignedTo]?.Value as string)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .OrderBy(x => x));
        }

        public WorkItem CodeRequest { get; }
        public ObservableCollection<WorkItem> Responses { get; }
        public ObservableCollection<WorkItem> Waiting { get; }

        public string ResponseStatuses { get; }

        public int ResponseCount => Responses.Count;
        public int WaitingCount => Waiting.Count;

        public string Reviewers { get; }
    }

    public class CodeRequestsViewModel : BindableExtended
    {
        private readonly IWorkItem _workItemService;
        private readonly IConnect _connectService;
        private bool _isBusy;
        private ObservableCollection<CodeReviewViewModel> _codeReviews;
        private ObservableCollection<CodeReviewViewModel> _selection;

        public CodeRequestsViewModel(IWorkItem workItemService, IConnect connectService)
        {
            _workItemService = workItemService;
            _connectService = connectService;

            UpdateCommand = ObservableCommand.FromAsyncHandler(OnUpdate);
            CloseInSelection = new ObservableCommand(OnCloseSelection, o => o is IList { Count: > 0 });

            OnUpdate();
        }

        private void OnCloseSelection(object obj)
        {
            if (obj is IList list && list.Count > 0)
            {
                foreach (var item in list.OfType<CodeReviewViewModel>().Select(x => x.CodeRequest))
                {
                    item.State = WorkItemStates.Closed;
                    _workItemService.SaveElement(item);
                }
                
                WindowManager.ShowBalloonSuccess(string.Format(Resources.AS_Mask_ReviewClosed, list.Count));

                OnUpdate();
            }
        }

        public ICommand CloseInSelection { get; }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ICommand UpdateCommand { get; }

        public ObservableCollection<CodeReviewViewModel> CodeReviews
        {
            get => _codeReviews;
            set => SetProperty(ref _codeReviews, value);
        }

        public ObservableCollection<CodeReviewViewModel> Selection
        {
            get => _selection;
            set => SetProperty(ref _selection, value);
        }

        private async Task OnUpdate()
        {
            IsBusy = true;

            var quarry = _workItemService.MyItemsQuarry +
                         // Ищем только Code Review Request
                         $"and {Sql.Fields.WorkItemType} = '{WorkItemTypes.CodeReview}'";

            var items = await Task.Run(() => _connectService.WorkItemStore.Query(quarry).OfType<WorkItem>().ToList());

            var allLinks = await Task.Run(() =>
                _workItemService.FindById(items.SelectMany(x =>
                    x.WorkItemLinks.OfType<WorkItemLink>().Select(y => y.TargetId))));
            

            var reviews = new List<CodeReviewViewModel>();
            
            foreach (var workItem in items)
            {
                var linked = workItem.WorkItemLinks.OfType<WorkItemLink>().Select(x => allLinks[x.TargetId]);

                reviews.Add(new CodeReviewViewModel(workItem, linked));
            }

            CodeReviews = new ObservableCollection<CodeReviewViewModel>(reviews);

            IsBusy = false;
        }
    }
}