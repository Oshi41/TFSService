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
        private readonly IChekins _api;
        private readonly IEqualityComparer<WorkItem> _comparer = new IdWorkItemComparer();
        private readonly List<WorkItem> _reviews;
        private readonly TimeSpan _time;
        private bool _isBusy;
        private DateTime _oldDate = DateTime.Today.AddDays(-7);
        private bool _isSelectingDate;

        public NewResponsesBaloonViewModel(IEnumerable<WorkItem> responses,
            IEnumerable<WorkItem> reviews,
            IChekins api,
            string title = null)
            : base(responses, title ?? Resources.AS_CodeReviewRequested)
        {
            _reviews = reviews.ToList();
            _api = api;

            _time = TimeSpan.FromDays(Settings.Settings.Read().OldReviewDay);

            CloseReviewes = ObservableCommand.FromAsyncHandler(OnCloseGoodLooking, OnCanCloseGoodLooking).ExecuteOnce();
            CloseOldRequests = ObservableCommand.FromAsyncHandler(OnCloseOldRequests, OnCanCloseOldRequests);
        }

        public ICommand CloseReviewes { get; }
        public ICommand CloseOldRequests { get; }

        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }

        public DateTime OldDate
        {
            get => _oldDate;
            set => Set(ref _oldDate, value);
        }

        public bool IsSelectingDate
        {
            get => _isSelectingDate;
            set => Set(ref _isSelectingDate, value);
        }


        private bool OnCanCloseGoodLooking()
        {
            return _reviews.Any(x => x.IsNotClosed());
        }

        private bool OnCanCloseOldRequests(object parameter)
        {
            if (parameter == null)
            {
                // Есть ли старый запросы проверки кода от кого-то на мне
                return Items.Select(x => x.Item).Any(x => IsOld(x.CreatedDate));
            }

            if (true.Equals(parameter))
            {
                return OldDate != DateTime.MinValue;
            }

            return true;
        }

        private async Task OnCloseOldRequests(object param)
        {
            if (param == null)
            {
                IsSelectingDate = true;
            }

            if (true.Equals(param))
            {
                param = false;
                
                IsBusy = false;

                var items = await Task.Run(() => _api.CloseRequests(x => IsOld(x.CreatedDate)));

                IsBusy = true;

                WindowManager.ShowBalloonSuccess(string.Format(Resources.AS_Mask_ResponseClosed, items.Count,
                    OldDate.ToLongDateString()));
            }

            if (false.Equals(param))
            {
                IsSelectingDate = false;
            }
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
            return createdDate > OldDate;
        }
    }
}