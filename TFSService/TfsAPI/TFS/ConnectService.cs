using System;
using System.Threading.Tasks;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Interfaces;
using TfsAPI.Logger;

namespace TfsAPI.TFS
{
    public class ConnectService : IConnect
    {
        private TfsTeamProjectCollection _tfs;
        private Project _project;

        public TfsTeamProjectCollection Tfs
        {
            get => _tfs;
            set
            {
                if (_tfs != value)
                {
                    _tfs = value;
                    Tfs.EnsureAuthenticated();
                    
                    // Не знаем к какому проекту подключились, сбрасываю
                    Project = null;
                    
                    WorkItemStore = Tfs.GetService<WorkItemStore>();
                    Linking = Tfs.GetService<ILinking>();
                    VersionControlServer = Tfs.GetService<VersionControlServer>();
                    ManagementService2 = Tfs.GetService<IIdentityManagementService2>();
                    TeamService = Tfs.GetService<TfsTeamService>();
                    Name = WorkItemStore.UserDisplayName;

                    TfsChanged?.Invoke(this, Tfs);
                    
                    LoggerHelper.WriteLine($"Connected to TFS {Tfs?.Uri}");
                }
            }
        }

        public WorkItemStore WorkItemStore { get; private set; }
        public ILinking Linking { get; private set; }
        public VersionControlServer VersionControlServer { get; private set; }
        public IIdentityManagementService2 ManagementService2 { get; private set; }
        public TfsTeamService TeamService { get; private set; }

        public Project Project
        {
            get => _project;
            set
            {
                if (_project != value)
                {
                    _project = value;
                    ProjectChanged?.Invoke(this, Project);
                    
                    LoggerHelper.WriteLine($"Connected to project {Project?.Name}");
                }
            }
        }

        public string Name { get; set; }

        public event EventHandler<Project> ProjectChanged;
        public event EventHandler<TfsTeamProjectCollection> TfsChanged;
        
        public void Connect(string url, string projectName = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("No tfs url provided", url);
            }

            Tfs = new TfsTeamProjectCollection(new Uri(url));

            if (!string.IsNullOrEmpty(projectName))
            {
                Project = WorkItemStore.Projects[projectName];
            }
        }
    }
}