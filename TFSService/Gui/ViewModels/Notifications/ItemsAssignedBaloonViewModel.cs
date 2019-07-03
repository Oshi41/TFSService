using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Gui.Properties;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm.Commands;
using TfsAPI.Extentions;

namespace Gui.ViewModels.Notifications
{
    /// <summary>
    ///     Уведомление о новых рабочих элементах
    /// </summary>
    public class ItemsAssignedBaloonViewModel : BindableNotificationBase
    {
        private WorkItemVm _selected;

        public ItemsAssignedBaloonViewModel(IEnumerable<WorkItem> e,
            string title = null)
            : base(title ?? Resources.AS_NewItemsAssigned)
        {
            Items = e.Select(x => new WorkItemVm(x)).ToList();
            OpenLinkCommand = new DelegateCommand(OpenLink, CanOpenLink);
        }

        /// <summary>
        ///     Список новых элементов
        /// </summary>
        public List<WorkItemVm> Items { get; }

        /// <summary>
        ///     Активный (выбранный) элемент
        /// </summary>
        public WorkItemVm Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }

        public ICommand OpenLinkCommand { get; }

        private void OpenLink()
        {
            var link = Selected?.Item?.Uri?.ToString();

            if (link != null)
            {
                Selected.Item.OpenLink();
            }
        }

        private bool CanOpenLink()
        {
            return Selected != null;
        }
    }
}