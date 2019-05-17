using System;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsAPI.Constants;
using TfsAPI.TFS;

namespace Tests
{
    [TestClass]
    public class TfsTests
    {
        [TestMethod]
        public void GetCapacity()
        {
            using (var tfs = new Tfs("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"))
            {
                var cap = tfs.GetCapacity();
            }
        }

        //[TestMethod]
        //public void GetLatestTask()
        //{
        //    using (var tfs = new Tfs("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"))
        //    {
        //        var latest = tfs.GetMyTasks().OrderByDescending(x => x.CreatedDate).FirstOrDefault();
        //    }
        //}

        //[TestMethod]
        //public void TryWriteHour()
        //{
        //    using (var tfs = new Tfs("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"))
        //    {
        //        var latest = tfs.GetMyTasks().OrderByDescending(x => x.CreatedDate).FirstOrDefault();

        //        tfs.WriteHours(latest, 1);
        //    }
        //}

        [TestMethod]
        public void GetMyQuarries()
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
        }

        [TestMethod]
        public void GetLinkedItems()
        {
            using (var tfs = new Tfs("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"))
            {
                var id = 40952;

                var items = tfs.GetAssociateItems(id);

            }
        }

        [TestMethod]
        public void GetMyItems()
        {
            using (var tfs = new Tfs("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"))
            {
                var all = tfs.GetMyWorkItems();
            }
        }

        [TestMethod]
        public void GetMyName()
        {
            var name = "Щеглов Аркадий";

            using (var project = new TfsTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security")))
            {
                Assert.AreEqual(name, project.AuthorizedIdentity.DisplayName);
            }
        }

        [TestMethod]
        public void GetAllLinkTypes()
        {
            using (var project = new TfsTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security")))
            {
                var store = project.GetService<WorkItemStore>();

                var childType = store.WorkItemLinkTypes.Select(x => x.ReferenceName).ToList();
            }
        }

        [TestMethod]
        public void GetTaskLinks()
        {
            using (var tfs = new Tfs("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"))
            {
                var items = tfs.GetMyWorkItems().Where(x => x.Type.Name.Equals(WorkItemTypes.Task)).ToList();

                foreach (var task in items)
                {
                    var name = task.Title;

                    foreach (Link link in task.Links)
                    {
                        
                    }
                }
            }
        }
    }
}
