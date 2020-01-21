using System.Windows.Input;
using Mvvm;
using Mvvm.Commands;
using Newtonsoft.Json;

namespace Gui.ViewModels
{
    public class ItemTypeMark : BindableBase
    {
        private bool _isChecked;
        private bool _isEnabled;
        private string _value;

        [JsonConstructor]
        public ItemTypeMark(string value, bool isChecked = true, bool isEnabled = true)
        {
            Value = value;
            IsEnabled = isEnabled;
            IsChecked = isChecked;

            CheckCommand = new DelegateCommand(() => IsChecked = !IsChecked, () => isEnabled);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        [JsonIgnore]
        public ICommand CheckCommand { get; }

        public static implicit operator ItemTypeMark(string name)
        {
            return new ItemTypeMark(name);
        }
    }
}