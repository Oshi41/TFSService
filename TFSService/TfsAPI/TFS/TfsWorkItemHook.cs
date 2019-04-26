using System;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Payloads;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.TFS
{
    class TfsWorkItemHook : VstsWebHookHandlerBase
    {
        public event EventHandler<int> Created;

        public override Task ExecuteAsync(WebHookHandlerContext context, WorkItemCreatedPayload payload)
        {
            Created?.Invoke(this, payload.Resource.Id);

            return Task.FromResult(true);
        }
    }
}
