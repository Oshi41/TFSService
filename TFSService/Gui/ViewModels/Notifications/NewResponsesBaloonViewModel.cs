using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Gui.Helper;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;
using static TfsAPI.Constants.WorkItems;

namespace Gui.ViewModels.Notifications
{
    class NewResponsesBaloonViewModel : ItemsAssignedBaloonViewModel
    {
        private readonly List<WorkItem> _reviews;
        private readonly ITfsApi _api;
        private bool _isExecuted;

        public NewResponsesBaloonViewModel(List<WorkItem> responses,
            List<WorkItem> reviews,
            ITfsApi api,
            string title = "У тебя запросили проверку кода")
            : base(responses, title)
        {
            _reviews = reviews;
            _api = api;

            CloseTheRestCommand = ObservableCommand.FromAsyncHandler(OnCloseReviews, OnCanClose);
        }

        private bool OnCanClose()
        {
            return !_isExecuted && _reviews.Any(x => x.IsNotClosed());
        }

        private async Task OnCloseReviews()
        {
            await Task.Run(() => _api.CloseCompletedReviews(ClosedStatus.LooksGood));
            _isExecuted = true;
        }

        public ICommand CloseTheRestCommand { get; set; }
    }
}
