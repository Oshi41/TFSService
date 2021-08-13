using Gui.Helper;
using Gui.Settings;
using Gui.ViewModels.DialogViewModels;
using Mvvm.Commands;

namespace Gui.ViewModels
{
    public class ObserveViewModel : BindableExtended
    {
        private bool _workItemObserve;
        private int _workItemDelay;
        private bool _buildsObserve;
        private int _buildsDelay;

        public ObserveViewModel()
        {
            SubmitCommand = new ObservableCommand(OnSubmit);

            using (var settings = new BuildsSettings().Read<BuildsSettings>())
            {
                BuildsDelay = (int)settings.Delay.TotalSeconds;
                BuildsObserve = settings.Observe;
            }

            using (var settings = new WorkItemSettings().Read<WorkItemSettings>())
            {
                WorkItemDelay = (int)settings.Delay.TotalSeconds;
                WorkItemObserve = settings.Observe;
            }
        }

        private void OnSubmit()
        {
        }

        public bool WorkItemObserve
        {
            get => _workItemObserve;
            set => SetProperty(ref _workItemObserve, value);
        }

        public int WorkItemDelay
        {
            get => _workItemDelay;
            set => SetProperty(ref _workItemDelay, value);
        }

        public bool BuildsObserve
        {
            get => _buildsObserve;
            set => SetProperty(ref _buildsObserve, value);
        }

        public int BuildsDelay
        {
            get => _buildsDelay;
            set => SetProperty(ref _buildsDelay, value);
        }
    }
}