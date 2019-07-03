using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Tfs_Api_Tests
{
    [TestClass]
    public class GetCheckinsTests : BaseTest
    {
        public static IEnumerable<object[]> TestData
            => new List<object[]>
            {
                // Один чекин
                new object[]
                {
                    new DateTime(2019, 6, 5),
                    new DateTime(2019, 6, 5),
                    new Func<int, bool>(i => i == 4)
                },

                // Будущая дата
                new object[]
                {
                    new DateTime(2200, 1, 1),
                    new DateTime(2200, 1, 1),
                    new Func<int, bool>(i => i == 0)
                },

                // Выходной
                new object[]
                {
                    new DateTime(2019, 6, 8),
                    new DateTime(2019, 6, 8),
                    new Func<int, bool>(i => i == 0)
                },

                // Один чекин
                new object[]
                {
                    new DateTime(2019, 6, 6),
                    new DateTime(2019, 6, 6),
                    new Func<int, bool>(i => i == 1)
                }
            };

        [DataTestMethod]
        [DynamicData(nameof(TestData))]
        public void GetWriteoffs(DateTime from, DateTime to, Func<int, bool> func)
        {
            var checkins = GetConn().GetWriteoffs(from, to);

            Assert.IsTrue(func(checkins.Count));
        }
    }
}