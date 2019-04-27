using System;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Payloads;

namespace TfsAPI.TFS
{
    class TfsWorkItemHook : VstsWebHookHandlerBase
    {
        /// <summary>
        /// Создание элемента
        /// </summary>
        public event EventHandler<int> Created;

        /// <summary>
        /// Изменение рабочего элемента
        /// </summary>
        public event EventHandler<int> Updated;

        #region Overrides of VstsWebHookHandlerBase

        public override Task ExecuteAsync(WebHookHandlerContext context, WorkItemCreatedPayload payload)
        {
            Created?.Invoke(this, payload.Resource.Id);

            return base.ExecuteAsync(context, payload);
        }

        public override Task ExecuteAsync(WebHookHandlerContext context, WorkItemUpdatedPayload payload)
        {
            Updated?.Invoke(this, payload.Resource.WorkItemId);

            return base.ExecuteAsync(context, payload);
        }

        #endregion
    }
}
