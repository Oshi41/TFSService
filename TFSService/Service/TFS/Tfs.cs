using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Service.TFS
{
    public class Tfs : IDisposable
    {
        #region Fields
        
        private readonly TfsTeamProjectCollection _project;

        #endregion

        public Tfs(string url)
        {
            _project = new TfsTeamProjectCollection(new Uri(url));
        }

        #region Methods

        /// <summary>
        /// Возвращает последние чекины за указанный промежуток времени
        /// </summary>
        public IList<Changeset> GetLatestCheckIns(TimeSpan fromNow)
        {
            var service = _project.GetService<VersionControlServer>();

            var parameters = new QueryHistoryParameters("*", RecursionType.Full)
            {
                Author = _project.AuthorizedIdentity.DisplayName,
            };

            var lastDate = DateTime.Now - fromNow.Duration();
            var changes = service.QueryHistory(parameters).Where(x => x.CreationDate >= lastDate);

            return changes.ToList();
        }

        public object GetLatestCreatedTask()
        {
            var service = _project.GetService<WorkItemStore>();

            //service.Query("SELECT * from WorkItemLinks WHERE []")
        }

        public void Dispose()
        {
            _project.Dispose();
        }

        #endregion
    }
}
