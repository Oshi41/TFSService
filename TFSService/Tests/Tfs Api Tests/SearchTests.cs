using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsAPI.TFS;

namespace Tests.Tfs_Api_Tests
{
    [TestClass]
    public class SearchTests
    {
        [TestMethod]
        public void TestSearch()
        {
            var tfs = new WorkItemService("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");

            var text = "Перевод";

            var result = tfs.Search(text);
            Assert.IsTrue(result.Any());
        }
    }
}