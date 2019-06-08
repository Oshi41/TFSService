namespace Gui.Tests
{
#if TESTS
    class TestDialogViewModel : BindableExtended
    {
        private readonly bool _setError;

        private bool needToShowError = false;

        public string Property { get; set; }

        public TestDialogViewModel(bool isAwait, bool setError)
        {
            _setError = setError;
            SubmitCommand = isAwait
                ? ObservableCommand.FromAsyncHandler(ExecuteMethod)
                : new ObservableCommand(() => ValidateProperty(nameof(Property)));
        }

        private async Task ExecuteMethod()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            needToShowError = true;
            ValidateProperty(nameof(Property));
            OnPropertyChanged(nameof(Error));
        }

        protected override string ValidateProperty(string prop)
        {
            if (needToShowError)
            {
                return "Return Error";
            }

            return base.ValidateProperty(prop);
        }
    }

#endif
}