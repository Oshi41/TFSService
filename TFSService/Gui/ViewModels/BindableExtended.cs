using Mvvm;
using System.ComponentModel;

namespace Gui.ViewModels
{
    class BindableExtended : BindableBase, IDataErrorInfo
    {
        public string this[string columnName] => ValidateProperty(columnName);

        public string Error => ValidateError();

        protected virtual string ValidateProperty(string prop)
        {
            return null;
        }

        protected virtual string ValidateError()
        {
            return null;
        }
    }
}
