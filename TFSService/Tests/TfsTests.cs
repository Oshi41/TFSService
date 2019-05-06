using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsAPI;

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

        //[TestMethod]
        //public void GetMyQuarries()
        //{
        //    // подключаемся к ТФС
        //    var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security"));

        //    // Выбираем проект
        //    var project = tfs.GetService<ITestManagementService>().GetTeamProject("SNES");

        //    var quarries = string.Empty;

        //    foreach (var query in project.Queries)
        //    {
        //        quarries += $"{query.Name}\n{query.QueryText}\n\n";
        //    }

        //}

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
    }
}
