using System;
using System.Diagnostics;
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
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
            {
                var cap = tfs.GetCapacity();
            }
        }

        //[TestMethod]
        //public void GetLatestTask()
        //{
        //    var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
        //    {
        //        var latest = tfs.GetMyTasks().OrderByDescending(x => x.CreatedDate).FirstOrDefault();
        //    }
        //}

        //[TestMethod]
        //public void TryWriteHour()
        //{
        //    var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
        //    {
        //        var latest = tfs.GetMyTasks().OrderByDescending(x => x.CreatedDate).FirstOrDefault();

        //        tfs.WriteHours(latest, 1);
        //    }
        //}

        [TestMethod]
        public void GetMyQuarries()
        {
            // подключаемся к ТФС
            var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(
                new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"));

            // Выбираем проект
            var project = tfs.GetService<ITestManagementService>().GetTeamProject("SNES");

            var quarries = string.Empty;

            foreach (var query in project.Queries) quarries += $"{query.Name}\n{query.QueryText}\n\n";

            Trace.WriteLine(quarries);
        }

        [TestMethod]
        public void GetLinkedItems()
        {
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
            {
                var id = 40952;

                var items = tfs.GetAssociateItems(id);
            }
        }

        [TestMethod]
        public void GetMyItems()
        {
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
            {
                var all = tfs.GetMyWorkItems();
            }
        }

        [TestMethod]
        public void GetMyName()
        {
            var name = "Щеглов Аркадий";

            using (var project =
                new TfsTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security")))
            {
                Assert.AreEqual(name, project.AuthorizedIdentity.DisplayName);
            }
        }

        [TestMethod]
        public void GetAllLinkTypes()
        {
            using (var project =
                new TfsTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security")))
            {
                var store = project.GetService<WorkItemStore>();

                var childType = store.WorkItemLinkTypes.Select(x => x.ReferenceName).ToList();
            }
        }

        [TestMethod]
        public void GetTaskLinks()
        {
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
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

        [TestMethod]
        public void CheckMyWriteOff()
        {
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
            {
                var from = DateTime.Today;

                var checkins = tfs.GetCheckins(from, from.Add(TimeSpan.FromDays(1)));

                Trace.WriteLine($"Today was - {checkins.Sum(x => x.Value)}");

                from = from.Add(TimeSpan.FromDays(-1));

                checkins = tfs.GetCheckins(from, from.Add(TimeSpan.FromDays(1)));

                Trace.WriteLine($"Yesterday was - {checkins.Sum(x => x.Value)}");
            }
        }

        [TestMethod]
        public void GetHours5()
        {
            var date = new DateTime(2019, 5, 6);
            var hours = 13;
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");

            var checkins = tfs.GetCheckins(date.AddDays(-1), date.AddDays(1));

            Assert.AreEqual(checkins.Select(x => x.Value).Sum(), hours);
        }

        [TestMethod]
        public void GetHours21()
        {
            var date = new DateTime(2019, 5, 21);
            var hours = 14;
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");

            var checkins = tfs.GetCheckins(date, date);

            Assert.AreEqual(checkins.Select(x => x.Value).Sum(), hours);
        }
    }
}