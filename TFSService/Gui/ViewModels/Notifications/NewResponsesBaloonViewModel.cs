using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Gui.Helper;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Comarers;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.Notifications
{
    /// <summary>
    /// Т.к. команды выполняются один раз, для нового выполнения требуется пересоздать этот объект
    /// </summary>
    public class NewResponsesBaloonViewModel : ItemsAssignedBaloonViewModel
    {
        private readonly IEqualityComparer<WorkItem> _comparer = new WorkItemComparer();

        private readonly ITfsApi _api;
        private readonly TimeSpan _time;
        private readonly List<WorkItem> _reviews;
        private bool isBusy;

        public NewResponsesBaloonViewModel(IEnumerable<WorkItem> responses,
            IEnumerable<WorkItem> reviews,
            ITfsApi api,
            string title = null)
            : base(responses, title ?? Properties.Resources.AS_CodeReviewRequested)
        {
            _reviews = reviews.ToList();
            _api = api;

            _time = TimeSpan.FromDays(Settings.Settings.Read().OldReviewDay);

            CloseReviewes = ObservableCommand.FromAsyncHandler(OnCloseGoodLooking, OnCanCloseGoodLooking).ExecuteOnce();
            CloseOldReviewes = ObservableCommand.FromAsyncHandler(OnCloseOld, OnCanCloseOld).ExecuteOnce();
        }

        public ICommand CloseReviewes { get; }
        public ICommand CloseOldReviewes { get; }

        public bool IsBusy { get => isBusy; set => Set(ref isBusy, value); }


        private bool OnCanCloseGoodLooking()
        {
            return _reviews.Any(x => x.IsNotClosed());
        }

        private bool OnCanCloseOld()
        {
            var now = DateTime.Now;

            return OnCanCloseGoodLooking()
                   && _reviews.Any(x => IsOld(x.CreatedDate));
        }

        private async Task OnCloseOld()
        {
            await CloseReviewesInner((request, responses) =>
            {
                if (responses.IsNullOrEmpty()
                    || request.HasState(WorkItemStates.Closed))
                    return false;

                // Что-то нуждается в доработке
                if (responses.Any(x => x.HasClosedReason(WorkItems.ClosedStatus.NeedsWork)))
                {
                    Trace.WriteLine($"Can't close {request.Id}, responses need work");
                    return false;
                }

                return IsOld(request.CreatedDate);
            }, _reviews);
        }

        private async Task OnCloseGoodLooking()
        {
            await CloseReviewesInner((request, responses) =>
            {
                if (responses.IsNullOrEmpty()
                    || request.HasState(WorkItemStates.Closed))
                    return false;

                return responses.All(x => x.HasClosedReason(WorkItems.ClosedStatus.LooksGood));
            }, _reviews);
        }

        private async Task CloseReviewesInner(CanCloseReview canClose, IList<WorkItem> source)
        {
            IsBusy = false;

            // Получил закрытые проверки кода
            var result = await Task.Run(() => _api.CloseCompletedReviews(canClose));

            // Ищу закрытые среди всех
            var toRemove = source.Intersect(result, _comparer).ToList();

            // удаляю такие вхождения
            toRemove.ForEach(x => source.Remove(x));
                        

            IsBusy = false;
        }


        /// <summary>
        ///     Вынес для гибкости дальнейшего функционала
        /// </summary>
        /// <param name="createdDate">Дата создания запроса кода</param>
        /// <returns></returns>
        private bool IsOld(DateTime createdDate)
        {
            var now = DateTime.Now;

            return (now - createdDate).Duration() > _time;
        }
    }
}