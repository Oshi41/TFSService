using System.Diagnostics;
using System.Windows.Input;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm.Commands;

namespace Gui.ViewModels
{
    /// <summary>
    ///     Обёртка для работы с TFS данными
    /// </summary>
    public class WorkItemVm
    {
        public WorkItemVm(WorkItem item)
        {
            Item = item;

            WebCommand = new DelegateCommand(OnNavigate, OnCanNavigate);
        }

        public WorkItem Item { get; }

        public ICommand WebCommand { get; }

        private void OnNavigate()
        {
            var link =
                $"{Item?.Collection?.Store?.TeamProjectCollection?.Uri}/{Item?.Project?.Name}/_workitems/edit/{Item?.Id}";
            Process.Start(link);
        }

        private bool OnCanNavigate()
        {
            return Item?.Collection?.Store?.TeamProjectCollection?.Uri != null
                   && Item.Project?.Name != null;
        }

        public override string ToString()
        {
            return Item?.Id.ToString() ?? string.Empty;
        }

        public static implicit operator WorkItemVm(WorkItem item)
        {
            return new WorkItemVm(item);
        }

        public static implicit operator WorkItem(WorkItemVm vm)
        {
            return vm?.Item;
        }
    }
}