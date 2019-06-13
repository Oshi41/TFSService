using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsAPI.Constants;
using TfsAPI.Rules;

namespace Tests
{
    [TestClass]
    public class BuilderTests
    {
        [TestMethod]
        public void BuildTest()
        {
            var a =  $"select * from {TfsAPI.Constants.Sql.Tables.WorkItems} " +
                   $"and {TfsAPI.Constants.Sql.Fields.WorkItemType} = '{WorkItemTypes.Task}' " +
                   $"and {TfsAPI.Constants.Sql.IsCurrentIteractionCondition}" +
                   $"and {Sql.AssignedToMeCondition} " +
                   $"and {Sql.Fields.State} <> {WorkItemStates.Closed}";


            var builder = new WiqlBuilder()
                .WithItemTypes("and", "=", WorkItemTypes.Task)
                .CurrentIteration()
                .AssignedToMe()
                .WithStates("and", "<>", WorkItemStates.Closed)
                .ToString();


            Assert.AreEqual(a, builder);
        }
    }
}
