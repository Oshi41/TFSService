using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Gui.Helper;
using Microsoft.TeamFoundation.Common;
using Mvvm.Commands;
using TfsAPI.Interfaces;
using TfsAPI.TFS;

namespace Gui.ViewModels.DialogViewModels
{
    public class FirstConnectionViewModel : BindableExtended
    {
        #region Fields

        private ITfs _tfs;

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
                if (SetProperty(ref _text, value))
                {
                    // Хак: при изменении текста возвращаем
                    // статус подключения в null,
                    // то есть никогда не подключались
                    IsConnected = null;
                }
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
                    return "Строка не является веб-адресом";
                }
            }

            return base.ValidateProperty(prop);
        }

        protected override string ValidateOptionalProperty(string prop)
        {
            if (prop == nameof(Text))
            {
                if (IsConnected == false)
                {
                    return "Не удалось подключиться";
                }
            }

            return base.ValidateOptionalProperty(prop);
        }

        #region Command handlers

        private bool CanConnect()
        {
            return _arbiter.IsFree()
                   && !string.IsNullOrWhiteSpace(Text);
        }

        private async Task Connect()
        {
            await _arbiter.DoAsync(() =>
            {
                try
                {
                    _tfs = new Tfs(Text);
                }
                catch (Exception e)
                {
                    Trace.Write(e);
                }
            });

            SafeExecute(() =>
            {
                IsConnected = _tfs != null;

                // необходимо для валидации данных
                OnPropertyChanged(nameof(Text));
                SubmitCommand.RaiseCanExecuteChanged();
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// После удачного подключения возвращает TFS API
        /// </summary>
        /// <returns></returns>
        public ITfs GetConnection()
        {
            return IsConnected == true
                ? _tfs
                : null;
        }

        #endregion
    }
}
