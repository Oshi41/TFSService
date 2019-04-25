using System;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service.TFS;

namespace Tests
{
    [TestClass]
    public class TfsTests
    {
        [TestMethod]
        public void GetLatestTask()
        {
            using (var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"))
            {
                var latest = tfs.GetMyTasks().OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            }
        }

        [TestMethod]
        public void TryWriteHour()
        {
            using (var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"))
            {
                var latest = tfs.GetMyTasks().OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                tfs.WriteHours(latest, 1);
            }
        }

        [TestMethod]
        public void Smth()
        {
            // подключаемся к ТФС
            var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"));

            // Выбираем проект
            var project = tfs.GetService<ITestManagementService>().GetTeamProject("SNES");

            var quarries = string.Empty;

            foreach (var query in project.Queries)
            {
                quarries += $"{query.Name}\n{query.QueryText}\n\n";
            }

            //return project;
        }
    }
}
