using System;
using System.Linq;
using Gui.ViewModels.Filter;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mvvm;
using Newtonsoft.Json;
using TfsAPI.Constants;
using TfsAPI.Extentions;

namespace Gui.ViewModels
{
    public class FilterViewModel : BindableBase
    {
        public CategoryFilterViewModel WorkTypes { get; }
        public CategoryFilterViewModel States { get; }
        public IgnoredItemsFilterViewModel Ignored { get; }

        public event EventHandler FilterChanged;

        [JsonConstructor]
        public FilterViewModel(CategoryFilterViewModel workTypes, CategoryFilterViewModel states,
            IgnoredItemsFilterViewModel ignored)
        {
            WorkTypes = workTypes ?? new CategoryFilterViewModel(
                            Properties.Resources.AS_Filter_WorkTypes,
                            new ItemTypeMark[]
                            {
                                WorkItemTypes.Task,
                                WorkItemTypes.Pbi,
                                WorkItemTypes.Bug,
                                WorkItemTypes.Improvement,
                                WorkItemTypes.Incident,
                                WorkItemTypes.Feature,
                                WorkItemTypes.CodeReview,
                                WorkItemTypes.ReviewResponse
                            }, true);


            States = states ?? new CategoryFilterViewModel(
                         Properties.Resources.AS_Filter_WorkItemStates,
                         new ItemTypeMark[]
                         {
                             WorkItemStates.New,
                             WorkItemStates.Active,
                             WorkItemStates.Resolved,
                             WorkItemStates.Requested
                         }, true);

            Ignored = ignored ?? new IgnoredItemsFilterViewModel(
                          Properties.Resources.AS_Filter_IgnoredIds,
                          new ItemTypeMark[0], true);

            WorkTypes.FilterChanged += OnFilterChanged;
            States.FilterChanged += OnFilterChanged;
            Ignored.FilterChanged += OnFilterChanged;
        }

        public FilterViewModel(FilterViewModel source)
            : this(source?.WorkTypes, source?.States, source?.Ignored)
        {
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool Accepted(WorkItemVm item)
        {
            return Accepted(item?.Item);
        }

        public bool Accepted(WorkItem item)
        {
            if (item == null)
                return false;

            if (WorkTypes.IsEnable && !item.IsTypeOf(WorkTypes.GetSelected()))
                return false;

            if (States.IsEnable)
            {
                var states = States.GetSelected();
                if (!states.Any(item.HasState))
                    return false;
            }

            if (Ignored.IsEnable)
            {
                if (Ignored.Marks.Where(x => x.IsChecked).Select(x => x.Value)
                    .Any(x => string.Equals(x, item.Id.ToString())))
                    return false;
            }

            return true;
        }
    }
}