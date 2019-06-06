using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Gui.Helper;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;
using static TfsAPI.Constants.WorkItems;

namespace Gui.ViewModels.Notifications
{
    public class NewResponsesBaloonViewModel : ItemsAssignedBaloonViewModel
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

            CloseReviewes = ObservableCommand.FromAsyncHandler(OnCloseGoodLooking, OnCanCloseGoodLooking);

            CloseOldReviewes = ObservableCommand.FromAsyncHandler(OnCloseOld, OnCanCloseOld);
        }

        public ICommand CloseReviewes { get; private set; }
        public ICommand CloseOldReviewes { get; private set; }


        private bool OnCanCloseGoodLooking()
        {
            return !_isExecuted && _reviews.Any(x => x.IsNotClosed());
        }

        private bool OnCanCloseOld()
        {
            var now = DateTime.Now;

            return OnCanCloseGoodLooking()              
                && _reviews.Any(x => IsOld(x.CreatedDate));
        }

        private async Task OnCloseOld()
        {
            await Task.Run(() => _api.CloseCompletedReviews((request, responses) =>
            {
                if (responses.IsNullOrEmpty()
                || request.HasState(WorkItemStates.Closed))
                {
                    return false;
                }

                // Что-то нуждается в доработке
                if (responses.Any(x => x.HasClosedReason(WorkItems.ClosedStatus.NeedsWork)))
                {
                    Trace.WriteLine($"Can't close {request.Id}, responses need work");
                    return false;
                }

                return IsOld(request.CreatedDate);
            }));
        }

        private async Task OnCloseGoodLooking()
        {
            await Task.Run(() => _api.CloseCompletedReviews((request, responses) =>
            {
                if (responses.IsNullOrEmpty()
                || request.HasState(WorkItemStates.Closed))
                {
                    return false;
                }

                return responses.All(x => x.HasClosedReason(WorkItems.ClosedStatus.LooksGood));
            }));
        }


        /// <summary>
        /// Вынес для гибкости дальнейшего функционала
        /// </summary>
        /// <param name="createdDate">Дата создания запроса кода</param>
        /// <returns></returns>
        private bool IsOld(DateTime createdDate)
        {
            var now = DateTime.Now;

            // Считаю старым 100-дневные запросы кода
            return (now - createdDate).Duration() > TimeSpan.FromDays(100);
        }
        
    }
}
