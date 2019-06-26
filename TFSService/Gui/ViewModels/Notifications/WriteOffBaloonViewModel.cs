using System.Diagnostics;
using System.Windows.Input;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm.Commands;
using TfsAPI.Interfaces;
using ToastNotifications.Core;

namespace Gui.ViewModels.Notifications
{
    internal class WriteOffBaloonViewModel : BindableNotificationBase
    {
        public WriteOffBaloonViewModel(string caption = null)
            : base(caption ?? Properties.Resources.AS_PlannedWriteoffTime)
        {
            OpenLinkCommand = new DelegateCommand(OnClick);
        }      

        public WriteOffBaloonViewModel(ScheduleWorkArgs e)
            : this()
        {
            Item = e.Item;
            Hours = e.Hours;
        }

        public byte Hours { get; }

        public WorkItem Item { get; }

        public ICommand OpenLinkCommand { get; }

        /// <summary>
        ///     Открываем ссылку на элемент
        /// </summary>
        /// <param name="obj"></param>
        private void OnClick()
        {
            if (Item?.Uri != null)
            {

            }
        }
    }
}