using Mvvm;
using Newtonsoft.Json;

namespace Gui.ViewModels
{
    public class ItemTypeMark : BindableBase
    {
        private bool _isChecked;
        private bool _isEnabled;
        private string _workType;

        [JsonConstructor]
        public ItemTypeMark(string workType, bool isChecked = true, bool isEnabled = true)
        {
            WorkType = workType;
            IsEnabled = isEnabled;
            IsChecked = isChecked;
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public string WorkType
        {
            get => _workType;
            set => SetProperty(ref _workType, value);
        }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        public static implicit operator ItemTypeMark(string name)
        {
            return new ItemTypeMark(name);
        }
    }
}