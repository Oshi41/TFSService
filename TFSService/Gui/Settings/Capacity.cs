using Mvvm;

namespace Gui.Settings
{
    public class Capacity : BindableBase
    {
        private int hours;
        private bool byUser;

        public int Hours { get => hours; set => SetProperty(ref hours, value); }
        public bool ByUser { get => byUser; set => SetProperty(ref byUser, value); }
    }
}
