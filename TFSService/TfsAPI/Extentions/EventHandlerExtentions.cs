using System;
using System.Linq;
using Microsoft.VisualStudio.Services.Common;

namespace TfsAPI.Extentions
{
    public static class EventHandlerExtentions
    {
        /// <summary>
        /// Отписываемся от всех подписчиков события
        /// </summary>
        public static void Unsubscribe<T>(this EventHandler<T> e)
        {
            e?.GetInvocationList()
                .OfType<EventHandler<T>>()
                .ForEach(x => e -= x);
        }
    }
}