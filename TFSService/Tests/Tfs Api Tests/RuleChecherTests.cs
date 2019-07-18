﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsAPI.RulesNew;

namespace Tests.Tfs_Api_Tests
{
    [TestClass]
    public class RuleChecherTests : BaseTest
    {
        [TestMethod]
        public void TestCurrentIteration()
        {
            var api = GetConn();
            var ruleBuilder = new RuleBuilder();

            var currentIter = ruleBuilder.BuildPresets(StaticRules.AllTasksIsCurrentIteration, null);

            var result = ruleBuilder.CheckInconsistant(new[] {currentIter}, api);
        }
    }
}