using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Gui.ViewModels.Filter
{
    public interface ICategoryFilterViewModel
    {
        bool IsEnable { get; set; }
        ObservableCollection<IItemTypeMark> Marks { get; }
        string Title { get; }
        [JsonIgnore]
        bool CanDisable { get; }

        event EventHandler FilterChanged;
    }
}