using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Interfaces;
using ToastNotifications.Core;

namespace Gui.ViewModels.Notifications
{
    internal class WriteOffBaloonViewModel : BindableNotificationBase
    {
        public WriteOffBaloonViewModel()
            : base(Properties.Resources.AS_PlannedWriteoffTime)
        {
        }

        public WriteOffBaloonViewModel(string caption)
            : base(caption)
        {
        }

        public WriteOffBaloonViewModel(ScheduleWorkArgs e)
            : this()
        {
            Item = e.Item;
            Hours = e.Hours;

            Options.NotificationClickAction = OnClick;
        }

        public byte Hours { get; }

        public WorkItem Item { get; }

        /// <summary>
        ///     Открываем ссылку на элемент
        /// </summary>
        /// <param name="obj"></param>
        private void OnClick(NotificationBase obj)
        {
            if (Item?.Uri != null) Process.Start(Item.Uri.ToString());
        }
    }
}