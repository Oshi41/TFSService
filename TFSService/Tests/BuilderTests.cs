﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var a = $"select * from {Sql.Tables.WorkItems} " +
                    $"and {Sql.Fields.WorkItemType} = '{WorkItemTypes.Task}' " +
                    $"and {Sql.IsCurrentIteractionCondition}" +
                    $"and {Sql.AssignedToMeCondition} " +
                    $"and {Sql.Fields.State} <> {WorkItemStates.Closed}";


            var builder = new WiqlBuilder()
                .WithItemTypes("and", "=", WorkItemTypes.Task)
                .CurrentIteration()
                .AssignedTo()
                .WithStates("and", "<>", WorkItemStates.Closed)
                .ToString();


            Assert.AreEqual(a, builder);
        }
    }
}