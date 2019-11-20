using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Gui.Helper;
using Gui.Properties;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Comparers;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.Notifications
{
    /// <summary>
    ///     Т.к. команды выполняются один раз, для нового выполнения требуется пересоздать этот объект
    /// </summary>
    public class NewResponsesBaloonViewModel : ItemsAssignedBaloonViewModel
    {
        private readonly ITfsApi _api;
        private readonly IEqualityComparer<WorkItem> _comparer = new IdWorkItemComparer();
        private readonly List<WorkItem> _reviews;
        private readonly TimeSpan _time;
        private bool _isBusy;

        public NewResponsesBaloonViewModel(IEnumerable<WorkItem> responses,
            IEnumerable<WorkItem> reviews,
            ITfsApi api,
            string title = null)
            : base(responses, title ?? Resources.AS_CodeReviewRequested)
        {
            _reviews = reviews.ToList();
            _api = api;

            _time = TimeSpan.FromDays(Settings.Settings.Read().OldReviewDay);

            CloseReviewes = ObservableCommand.FromAsyncHandler(OnCloseGoodLooking, OnCanCloseGoodLooking).ExecuteOnce();
            CloseOldRequests = ObservableCommand.FromAsyncHandler(OnCloseOldRequests, OnCanCloseOldRequests).ExecuteOnce();
        }

        public ICommand CloseReviewes { get; }
        public ICommand CloseOldRequests { get; }

        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }


        private bool OnCanCloseGoodLooking()
        {
            return _reviews.Any(x => x.IsNotClosed());
        }

        private bool OnCanCloseOldRequests()
        {
            // Есть ли старый запросы проверки кода от кого-то на мне
            return Items.Select(x => x.Item).Any(x => IsOld(x.CreatedDate));
        }

        private async Task OnCloseOldRequests()
        {
            IsBusy = false;

            var items = await Task.Run(() => _api.CloseRequests(x => IsOld(x.CreatedDate)));

            IsBusy = true;

            var edge = DateTime.Now -_time;

            WindowManager.ShowBalloonSuccess(string.Format(Resources.AS_Mask_ResponseClosed, items.Count, edge.ToLongDateString()));
        }

        private async Task OnCloseGoodLooking()
        {
            IsBusy = false;

            // Получил закрытые проверки кода
            var result = await Task.Run(() => _api.CloseCompletedReviews((request, responses) =>
            {
                if (responses.IsNullOrEmpty()
                    || request.HasState(WorkItemStates.Closed))
                    return false;

                return responses.All(x => x.HasClosedReason(WorkItems.ClosedStatus.LooksGood));
            }));

            // Ищу закрытые среди всех
            var toRemove = _reviews.Intersect(result, _comparer).ToList();

            // удаляю такие вхождения
            toRemove.ForEach(x => _reviews.Remove(x));

            IsBusy = false;

            WindowManager.ShowBalloonSuccess(string.Format(Properties.Resources.AS_Mask_ReviewClosed, result.Count));
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