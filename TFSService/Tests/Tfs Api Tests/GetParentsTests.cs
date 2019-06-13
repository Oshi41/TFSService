using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Tfs_Api_Tests
{
    [TestClass]
    public class GetParentsTests : BaseTest
    {
        [DataTestMethod]
        [DataRow(75148, 2)]

        // Некорректные значения
        [DataRow(0, 0)]
        [DataRow(-1, 0)]
        public void GetParents(int id, int count)
        {
            var conn = GetConn();
            var item = conn.FindById(id);
            var parents = conn.GetParents(new[] {item});

            Assert.AreEqual(count, parents.Count);
        }
    }
}
