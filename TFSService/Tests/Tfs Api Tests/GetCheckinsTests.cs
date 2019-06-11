using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Tfs_Api_Tests
{
    [TestClass]
    public class GetCheckinsTests : BaseTest
    {
        [DataTestMethod]
        [DynamicData(nameof(TestData))]
        public void GetCheckins(DateTime from, DateTime to, Func<int, bool> func)
        {
            var checkins = GetConn().GetCheckins(from, to);

            Assert.IsTrue(func(checkins.Count));
        }

        public static IEnumerable<object[]> TestData
            => new List<object[]>
            {
                // Будущая дата
                new object[]
                {
                    new DateTime(2200,1,1),
                    new DateTime(2200,1,1),
                    new Func<int, bool>(i => i == 0),
                }
            };
    }
}
