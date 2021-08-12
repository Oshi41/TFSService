using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Constants;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;
using TfsAPI.Logger;

namespace TfsAPI.TFS
{
    public class CheckinsService : IChekins
    {
        private readonly IConnect _connect;
        private readonly IWorkItem _workItemService;

        public CheckinsService(IConnect connect, IWorkItem workItemService)
        {
            _connect = connect;
            _workItemService = workItemService;
        }

        #region ITfsApi

        public List<WorkItem> CloseCompletedReviews(CanCloseReview canClose)
        {
            if (canClose == null)
                throw new ArgumentException(nameof(canClose));

            var quarry = _workItemService.MyItemsQuarry +
                         // Ищем только Code Review Request
                         $"and {Sql.Fields.WorkItemType} = '{WorkItemTypes.CodeReview}'";

            var requests = _connect.WorkItemStore.Query(quarry).OfType<WorkItem>().ToList();

            LoggerHelper.WriteLineIf(requests.Any(), $"Founded {requests.Count} requests");

            var result = new List<WorkItem>();

            var all = _workItemService.FindById(requests.SelectMany(x => x
                .WorkItemLinks
                .OfType<WorkItemLink>()
                .Select(y => y.TargetId)));

            foreach (var request in requests)
            {
                var parents = request
                    .WorkItemLinks
                    .OfType<WorkItemLink>()
                    .Select(x => x.TargetId)
                    .ToList();

                var responses = all.Where(x => parents.Contains(x.Key)
                                               && x.Value.IsTypeOf(WorkItemTypes.ReviewResponse))
                    .Select(x => x.Value)
                    .ToList();

                LoggerHelper.WriteLine($"Request {request.Id} has {responses.Count} responses");

                if (canClose(request, responses)) result.Add(request);
            }

            foreach (var item in result)
            {
                item.State = WorkItemStates.Closed;
                _workItemService.SaveElement(item);
            }

            LoggerHelper.WriteLineIf(result.Any(), $"Closed {result.Count} requests");

            return result;
        }

        public List<WorkItem> CloseRequests(Predicate<WorkItem> canClose)
        {
            if (canClose == null)
                throw new ArgumentException(nameof(canClose));

            var quarry = _workItemService.MyItemsQuarry +
                         // Ищем запрошенные у меня проверки кода
                         $"and {Sql.Fields.WorkItemType} = '{WorkItemTypes.ReviewResponse}'";

            var items = _connect.WorkItemStore.Query(quarry).OfType<WorkItem>().ToList();

            LoggerHelper.WriteLineIf(items.Any(), $"Founded {items.Count} requests assigned to me");

            var canBeClosed = items.Where(x => canClose(x)).ToList();

            foreach (var item in canBeClosed)
            {
                if (!item.IsOpen)
                    item.Open();

                item.Fields[WorkItems.Fields.ClosedStatus].Value = WorkItems.ClosedStatus.LooksGood;
                item.State = WorkItemStates.Closed;
                _workItemService.SaveElement(item);
            }

            return canBeClosed;
        }

        public List<Changeset> GetCheckins(DateTime from, DateTime to, string user = null)
        {
            var innerResults = _connect.VersionControlServer.QueryHistory(@"$\",
                VersionSpec.Latest,
                0,
                RecursionType.Full,
                user,
                new DateVersionSpec(@from),
                new DateVersionSpec(to),
                int.MaxValue,
                false,
                false);

            return innerResults.OfType<Changeset>().ToList();
        }

        #endregion
    }
}