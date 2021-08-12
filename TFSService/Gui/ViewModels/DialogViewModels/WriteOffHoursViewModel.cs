using Gui.Properties;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.DialogViewModels
{
    public class WriteOffHoursViewModel : BindableExtended
    {
        private int _hours;

        public WriteOffHoursViewModel(IWorkItem api)
        {
            ChooseTaskVm = new ChooseTaskViewModel(api);

            SpecialCommand = ChooseTaskVm.SpecialCommand;
            SubmitCommand = ChooseTaskVm.SubmitCommand;
        }

        public ChooseTaskViewModel ChooseTaskVm { get; }

        public int Hours
        {
            get => _hours;
            set => SetProperty(ref _hours, value);
        }

        protected override string ValidateProperty(string prop)
        {
            if (string.Equals(prop, nameof(Hours))
                && Hours < 1)
                return Resources.AS_EnterWriteOffHours;

            return base.ValidateProperty(prop);
        }
    }
}