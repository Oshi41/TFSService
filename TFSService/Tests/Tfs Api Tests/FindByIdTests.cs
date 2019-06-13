using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsAPI.Constants;

namespace Tests.Tfs_Api_Tests
{
    [TestClass]
    public class FindByIdTests : BaseTest
    {
        [DataTestMethod]
        [DataRow(1265, WorkItemTypes.Bug)]
        [DataRow(6108, WorkItemTypes.Pbi)]
        [DataRow(35259, WorkItemTypes.Task)]
        public void FindByIdAndCheckTheType(int id, string type)
        {
            var item = GetConn().FindById(id);

            Assert.AreEqual(item.Type.Name,type);
        }

        [DataTestMethod]
        [DataRow(-1, WorkItemTypes.Bug)]
        [DataRow(0, WorkItemTypes.Pbi)]
        [DataRow(1265, "123123")]
        public void FindById_Incorrect(int id, string type)
        {
            var item = GetConn().FindById(id);

            Assert.AreNotEqual(item?.Type?.Name, type);
        }
    }
}
