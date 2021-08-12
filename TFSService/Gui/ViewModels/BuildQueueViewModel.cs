using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using Gui.Helper;
using Gui.ViewModels.DialogViewModels;
using Microsoft.TeamFoundation.Build.WebApi;
using Mvvm;
using Mvvm.Commands;
using TfsAPI.Interfaces;
using TfsAPI.Logger;

namespace Gui.ViewModels
{
    public class Parameter : BindableBase
    {
        private string _name;
        private string _value;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }

    public class Parameters : ObservableCollection<Parameter>
    {
        public Parameters()
        {
        }

        public Parameters(IEnumerable<Parameter> s) : base(s)
        {
        }

        public override string ToString()
        {
            return $"{{{string.Join(",", this.Select(x => $"\"{x.Name}\":\"{x.Value}\""))}}}";
        }
    }

    public class BuildQueueViewModel : BindableExtended
    {
        private readonly IBuild _buildService;
        private ObservableCollection<Build> _agentQueue = new ObservableCollection<Build>();
        private ObservableCollection<Build> _ownQueue = new ObservableCollection<Build>();
        private string _currentBuildName;
        private ObservableCollection<string> _allDefinitions;
        private Parameters _currentParams;
        private bool _isOpen;
        private readonly ICommand _addCommand;
        private readonly ICommand _removeCommand;
        private readonly ICommand _submitBuildCommand;
        private readonly ICommand _denyBuildCommand;
        private Build _selected;
        private Dictionary<BuildDefinitionReference, IDictionary<string, BuildDefinitionVariable>> _allRefs;
        private bool _busy;
        private readonly IConnect _connectService;

        private bool _forcedBuild = false;

        public BuildQueueViewModel(IBuild buildService, IConnect connectService)
        {
            _buildService = buildService;
            _connectService = connectService;

            _addCommand = new DelegateCommand(OnAdd);
            _removeCommand = new DelegateCommand(OnRemove, () => Selected != null);
            _denyBuildCommand = new DelegateCommand(() => IsOpen = false);
            _submitBuildCommand = DelegateCommand.FromAsyncHandler(OnAddBuild, OnCanAnnBuild);
            UpdateCommand = DelegateCommand.FromAsyncHandler(OnRefresh, () => !Busy);
            AddBuildCommand = new DelegateCommand(QueueBuild);

            OnRefresh();
        }

        private void QueueBuild()
        {
            _forcedBuild = true;
            IsOpen = true;
        }
        private async Task OnRefresh()
        {
            Busy = true;

            try
            {
                var builds = await Task.Run(() => _buildService.GetRunningBuilds());
                var references = await Task.Run(() => _buildService.GetAllDefentitions());

                var dictionary = references.ToDictionary(x => x, x => _buildService.GetDefaultProperties(x));
                await Task.WhenAll(dictionary.Values);

                _allRefs = dictionary.ToDictionary(x => x.Key, x => x.Value.Result);

                AgentQueue = new ObservableCollection<Build>(builds);
                AllDefinitions = new ObservableCollection<string>(_allRefs.Keys.Select(x => x.Name));

                using (var settings = Settings.Settings.Read())
                {
                    var source = settings?.QueuedBuilds?.ToList() ?? new List<Build>();
                    OwnQueue = new ObservableCollection<Build>(source);
                }
            }
            catch (Exception e)
            {
                LoggerHelper.WriteLine(e);
                WindowManager.ShowBalloonError(e.Message);
            }
            finally
            {
                Busy = false;
            }
        }

        private bool OnCanAnnBuild()
        {
            return !string.IsNullOrEmpty(CurrentBuildName);
        }

        private async Task OnAddBuild()
        {
            IDictionary<string,BuildDefinitionVariable> param;

            param = CurrentParams.ToDictionary(x => x.Name, x => new BuildDefinitionVariable
            {
                Value = x.Value
            });

            var build = await _buildService.Schedule(_connectService.Project.Name, CurrentBuildName, param, _forcedBuild);
            
            IsOpen = false;

            if (!_forcedBuild)
            {
                OwnQueue.Add(build);
            }
            else
            {
                await OnRefresh();
            }
        }

        private void OnRemove()
        {
            OwnQueue.Remove(Selected);
            Selected = OwnQueue.FirstOrDefault();
        }

        private void OnAdd()
        {
            _forcedBuild = false;
            IsOpen = true;
        }

        public ObservableCollection<Build> AgentQueue
        {
            get => _agentQueue;
            set => SetProperty(ref _agentQueue, value);
        }

        public ObservableCollection<Build> OwnQueue
        {
            get => _ownQueue;
            set => SetProperty(ref _ownQueue, value);
        }

        public string CurrentBuildName
        {
            get => _currentBuildName;
            set
            {
                if (SetProperty(ref _currentBuildName, value))
                {
                    var reference = _allRefs.Keys.FirstOrDefault(x => x.Name == CurrentBuildName);
                    if (reference != null)
                    {
                        var parameters = _allRefs[reference].Select(x => new Parameter
                        {
                            Name = x.Key,
                            Value = x.Value.Value
                        });

                        CurrentParams = new Parameters(parameters);
                        return;
                    }

                    CurrentParams.Clear();
                }
            }
        }

        public ObservableCollection<string> AllDefinitions
        {
            get => _allDefinitions;
            set => SetProperty(ref _allDefinitions, value);
        }

        public Parameters CurrentParams
        {
            get => _currentParams;
            set => SetProperty(ref _currentParams, value);
        }

        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        public ICommand AddCommand => _addCommand;

        public ICommand RemoveCommand => _removeCommand;

        public ICommand SubmitBuildCommand => _submitBuildCommand;

        public ICommand DenyBuildCommand => _denyBuildCommand;

        public Build Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        public bool Busy
        {
            get => _busy;
            set => SetProperty(ref _busy, value);
        }

        public ICommand UpdateCommand { get; }

        public ICommand AddBuildCommand { get; }
    }
}