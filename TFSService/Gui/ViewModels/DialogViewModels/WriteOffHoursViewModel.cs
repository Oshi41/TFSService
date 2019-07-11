using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    public class WriteOffHoursViewModel : BindableExtended
    {
        private int _hours;
        public ChooseTaskViewModel ChooseTaskVm { get; private set; }

        public int Hours
        {
            get => _hours;
            set => SetProperty(ref _hours, value);
        }

        public WriteOffHoursViewModel(ITfsApi api)
        {
            ChooseTaskVm = new ChooseTaskViewModel(api);

            SpecialCommand = ChooseTaskVm.SpecialCommand;
            SubmitCommand = ChooseTaskVm.SubmitCommand;
        }

        protected override string ValidateProperty(string prop)
        {
            if (string.Equals(prop, nameof(Hours))
                && Hours < 1)
            {
                return Properties.Resources.AS_EnterWriteOffHours;
            }

            return base.ValidateProperty(prop);
        }
    }
}
