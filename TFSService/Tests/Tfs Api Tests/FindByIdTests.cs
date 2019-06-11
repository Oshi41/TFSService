using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsAPI.Constants;

namespace Tests.Tfs_Api_Tests
{
    [TestClass]
    public class FindByIdTests : BaseTest
    {

        // TODO вставить конкретные данные!
        [DataTestMethod]
        [DataRow(1, WorkItemTypes.Bug)]
        [DataRow(2, WorkItemTypes.Pbi)]
        [DataRow(3, WorkItemTypes.Task)]
        public void FindByIdAndCheckTheType(int id, string type)
        {
            var item = GetConn().FindById(id);

            Assert.AreEqual(item.Type.Name,type);
        }

        // TODO вставить конкретные данные!
        [DataTestMethod]
        [DataRow(-1, WorkItemTypes.Bug)]
        [DataRow(0, WorkItemTypes.Pbi)]
        [DataRow(75145, "123123")]
        public void FindById_Incorrect(int id, string type)
        {
            var item = GetConn().FindById(id);

            Assert.AreNotEqual(item?.Type?.Name, type);
        }
    }
}
