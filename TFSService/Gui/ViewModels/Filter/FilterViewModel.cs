using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Gui.ViewModels.Filter;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
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

        public event EventHandler FilterChanged;

        [JsonConstructor]
        public FilterViewModel(CategoryFilterViewModel states, CategoryFilterViewModel workTypes)
        {
            WorkTypes = states ?? new CategoryFilterViewModel(
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


            States = workTypes ?? new CategoryFilterViewModel(
                            Properties.Resources.AS_Filter_WorkItemStates,
                            new ItemTypeMark[]
                            {
                                WorkItemStates.New,
                                WorkItemStates.Active,
                                WorkItemStates.Resolved,
                                WorkItemStates.Requested
                            }, true);

            WorkTypes.FilterChanged += OnFilterChanged;
            States.FilterChanged += OnFilterChanged;
        }

        public FilterViewModel(FilterViewModel source)
            : this(source?.States, source?.WorkTypes)
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

            return true;
        }
    }
}