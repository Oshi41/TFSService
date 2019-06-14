using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TfsAPI.TFS;

namespace Tests.Tfs_Api_Tests
{
    [TestClass]
    public class SearchTests
    {
        [TestMethod]
        public void TestSearch()
        {
            var tfs = new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");

            var text = "Перевод";

            var result = tfs.Search(text);
            Assert.IsTrue(result.Any());
        }
    }
}
