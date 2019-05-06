using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gui.ViewModels
{
    class TextInputVm : BindableExtended
    {
        private readonly Func<string, string> _validate;
        private string _text;
        private string _help;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public string Help
        {
            get => _help;
            set => SetProperty(ref _help, value);
        }

        public TextInputVm(string help, Func<string, string> validate)
        {
            Help = help;
            _validate = validate;
        }

        protected override string ValidateProperty(string prop)
        {
            if (prop == nameof(Text))
            {
                return _validate?.Invoke(Text);
            }

            return base.ValidateProperty(prop);
        }
    }
}
