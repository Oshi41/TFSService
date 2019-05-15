using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gui.Helper;
using Gui.ViewModels.DialogViewModels;
using Mvvm;

namespace Tests
{
    class TestDialogViewModel : BindableExtended
    {
        private readonly bool _setError;

        public string Property { get; set; }

        public TestDialogViewModel(bool await, bool setError)
        {
            _setError = setError;
            SubmitCommand = @await
                ? ObservableCommand.FromAsyncHandler(ExecuteMethod)
                : new ObservableCommand(() => ValidateProperty(nameof(Property)));
        }

        private async Task ExecuteMethod()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            ValidateProperty(nameof(Property));
        }

        protected override string ValidateProperty(string prop)
        {
            if (_setError)
            {
                return "Return Error";
            }

            return base.ValidateProperty(prop);
        }
    }
}
