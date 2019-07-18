namespace Gui.Tests
{
#if TESTS
    internal class TestDialogViewModel : BindableExtended
    {
        private readonly bool _setError;

        private bool _needToShowError;

        public TestDialogViewModel(bool isAwait, bool setError)
        {
            _setError = setError;
            SubmitCommand = isAwait
                ? ObservableCommand.FromAsyncHandler(ExecuteMethod)
                : new ObservableCommand(() => ValidateProperty(nameof(Property)));
        }

        public string Property { get; set; }

        private async Task ExecuteMethod()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            _needToShowError = true;
            ValidateProperty(nameof(Property));
            OnPropertyChanged(nameof(Error));
        }

        protected override string ValidateProperty(string prop)
        {
            if (_needToShowError) return "Return Error";

            return base.ValidateProperty(prop);
        }
    }

#endif
}