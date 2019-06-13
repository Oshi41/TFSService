using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Tfs_Api_Tests
{
    [TestClass]
    public class GetAssociateItemsTests : BaseTest
    {
        // Todo вставить верное значение!
        [DataTestMethod]
        [DataRow(41914)]
        public void FindRelated(int id)
        {
            var changesets = GetConn().GetAssociateItems(id);

            Assert.IsTrue(changesets.Any());
        }


        // Todo вставить верное значение!
        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(int.MaxValue)]
        // Чекин без связей
        [DataRow(40470)]
        public void FindRelated_IncorrectData_Empty(int id)
        {
            var changesets = GetConn().GetAssociateItems(id);

            Assert.IsFalse(changesets.Any());
        }
    }
}
