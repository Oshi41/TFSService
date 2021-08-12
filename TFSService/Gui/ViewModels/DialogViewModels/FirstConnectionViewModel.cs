using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Gui.Helper;
using Gui.Properties;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Interfaces;
using TfsAPI.TFS;

namespace Gui.ViewModels.DialogViewModels
{
    /// <summary>
    ///     Окошко с выбором подключения к TFS
    /// </summary>
    public class FirstConnectionViewModel : BindableExtended
    {
        private readonly IConnect _connectService;

        public FirstConnectionViewModel(IConnect connectService)
        {
            _connectService = connectService;
            
            using (var settings = Settings.Settings.Read())
            {
                RememberedConnections = settings.Connections?.ToList() ?? new List<string>();

                ProjectName = settings.Project;

                if (!RememberedConnections.IsNullOrEmpty())
                    Text = RememberedConnections.First();
            }

            CheckConnectionCommand = ObservableCommand.FromAsyncHandler(CheckConnect, CanCheckConnect);
            SubmitCommand = new ObservableCommand(() => { },
                () => Connection == ConnectionType.Success && _connectService.Project != null);
        }

        protected override string ValidateProperty(string prop)
        {
            if (prop == nameof(Text))
            {
                if (!Uri.TryCreate(Text, UriKind.Absolute, out var result)) return Resources.AS_NotAWebAddress_Error;

                if (Connection == ConnectionType.Failed) return Resources.AS_ConnectError;
            }

            if (prop == nameof(ProjectName))
            {
                switch (Connection)
                {
                    case ConnectionType.Failed:
                        return "";
                    
                    case ConnectionType.Success:
                        if (string.IsNullOrWhiteSpace(ProjectName))
                        {
                            return Resources.AS_ChooseProject;
                        }
                        break;
                }
            }

            return base.ValidateProperty(prop);
        }

        #region Fields

        private string _text;
        private IList<string> _rememberedConnections;
        private readonly ActionArbiterAsync _arbiter = new ActionArbiterAsync();
        private ConnectionType _connection = ConnectionType.Unknown;
        private ObservableCollection<string> _allProjects;
        private string _projectName;

        #endregion

        #region Properties

        public string Text
        {
            get => _text;
            set
            {
                if (SetProperty(ref _text, value)
                    && Connection == ConnectionType.Success)
                    // Сбрасываем подключение
                    Connection = ConnectionType.Unknown;
            }
        }

        public IList<string> RememberedConnections
        {
            get => _rememberedConnections;
            set => SetProperty(ref _rememberedConnections, value);
        }

        public ConnectionType Connection
        {
            get => _connection;
            set
            {
                if (SetProperty(ref _connection, value))
                    // необходимо для валидации данных
                    OnPropertyChanged(nameof(Text));
                //SubmitCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCommand CheckConnectionCommand { get; }
        
        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (SetProperty(ref _projectName, value) 
                    && Connection == ConnectionType.Success
                    && !string.IsNullOrWhiteSpace(ProjectName))
                {
                    _connectService.Project = _connectService.WorkItemStore.Projects[ProjectName];
                }
            }
        }

        public ObservableCollection<string> AllProjects
        {
            get => _allProjects;
            set => SetProperty(ref _allProjects, value);
        }

        #endregion

        #region Command handlers

        private bool CanCheckConnect()
        {
            return _arbiter.IsFree()
                   && !string.IsNullOrWhiteSpace(Text);
        }

        private async Task CheckConnect()
        {
            _connectService.Connect(Text, ProjectName);

            Connection = _connectService.Tfs != null 
                ? ConnectionType.Success 
                : ConnectionType.Failed;
            
            if (_connectService.WorkItemStore == null)
            {
                AllProjects.Clear();
            }
            else
            {
                AllProjects = new ObservableCollection<string>(_connectService.WorkItemStore.Projects.OfType<Project>().Select(x => x.Name));
            }

            CheckConnectionCommand.RaiseCanExecuteChanged();
            SubmitCommand.RaiseCanExecuteChanged();
        }

        #endregion

        public async Task<bool> TryConnect()
        {
            if (CanCheckConnect())
            {
                await CheckConnect();
                return _connectService.Project != null;
            }

            return false;
        }
    }

    /// <summary>
    ///     Состояние соединения.
    /// </summary>
    public enum ConnectionType
    {
        /// <summary>
        ///     Состояние неизвестно.
        /// </summary>
        Unknown,

        /// <summary>
        ///     Соединение успешно установленно.
        /// </summary>
        Success,

        /// <summary>
        ///     Во время соединения произошла ошибка.
        /// </summary>
        Failed
    }
}