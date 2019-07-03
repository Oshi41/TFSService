using System.Linq;
using System.Reflection;
using Gui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Setup;

namespace Tests
{
    [TestClass]
    public class NameTests
    {
        private const string Name = "TFS Service";

        [TestMethod]
        public void TestGuiName()
        {
            var name = typeof(App)
                .GetTypeInfo()
                .Assembly
                .GetCustomAttributes(typeof(AssemblyTitleAttribute))
                .OfType<AssemblyTitleAttribute>()
                .First();

            Assert.AreEqual(Name, name.Title);
        }

        [TestMethod]
        public void TestSetup()
        {
            Assert.AreEqual(Name, Register.AppName);
        }

        [TestMethod]
        public void AssemblyNameTest()
        {
            var name = nameof(Gui) + ".exe";

            Assert.AreEqual(Register.ExeName, name);
        }
    }
}