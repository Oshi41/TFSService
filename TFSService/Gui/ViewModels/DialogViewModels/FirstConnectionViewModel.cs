using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gui.Helper;
using Gui.Properties;
using Microsoft.TeamFoundation.Common;
using TfsAPI.TFS;

namespace Gui.ViewModels.DialogViewModels
{
    /// <summary>
    ///     Окошко с выбором подключения к TFS
    /// </summary>
    public class FirstConnectionViewModel : BindableExtended
    {
        public FirstConnectionViewModel()
        {
            using (var settings = Settings.Settings.Read())
            {
                RememberedConnections = settings.Connections?.ToList() ?? new List<string>();

                if (!RememberedConnections.IsNullOrEmpty())
                    Text = RememberedConnections.First();
            }

            SubmitCommand = ObservableCommand.FromAsyncHandler(Connect, CanConnect);
        }

        protected override string ValidateProperty(string prop)
        {
            if (prop == nameof(Text))
            {
                if (!Uri.TryCreate(Text, UriKind.Absolute, out var result))
                {
                    return Resources.AS_NotAWebAddress_Error;
                }

                if (IsConnected == false)
                {
                    return Resources.AS_ConnectError;
                }
            }

            return base.ValidateProperty(prop);
        }

        protected override string ValidateOptionalProperty(string prop)
        {
            if (prop == nameof(Text))
                if (IsConnected == false)
                    return Resources.AS_ConnectError;

            return base.ValidateOptionalProperty(prop);
        }

        #region Fields

        private string _text;
        private IList<string> _rememberedConnections;
        private readonly ActionArbiterAsync _arbiter = new ActionArbiterAsync();
        private bool? _isConnected;

        #endregion

        #region Properties

        public string Text
        {
            get => _text;
            set
            {
                if (SetProperty(ref _text, value)) IsConnected = null;
            }
        }

        public IList<string> RememberedConnections
        {
            get => _rememberedConnections;
            set => SetProperty(ref _rememberedConnections, value);
        }

        public bool? IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        #endregion

        #region Command handlers

        public bool CanConnect()
        {
            return _arbiter.IsFree()
                   && !string.IsNullOrWhiteSpace(Text);
        }

        public async Task Connect()
        {
            var connected = await TfsApi.CheckConnection(Text);

            SafeExecute(() =>
            {
                IsConnected = connected;

                // необходимо для валидации данных
                OnPropertyChanged(nameof(Text));
                SubmitCommand.RaiseCanExecuteChanged();
            });
        }

        #endregion
    }
}