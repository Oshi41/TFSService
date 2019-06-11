using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class NameTests
    {
        private const string _name = "TFS Service";

        [TestMethod]
        public void TestGuiName()
        {
            var name = typeof(Gui.App)
                .GetTypeInfo()
                .Assembly
                .GetCustomAttributes(typeof(AssemblyTitleAttribute))
                .OfType<AssemblyTitleAttribute>()
                .First();

            Assert.AreEqual(_name, name.Title);
        }

        [TestMethod]
        public void TestSetup()
        {
            Assert.AreEqual(_name, Setup.Register.AppName);
        }

        [TestMethod]
        public void AssemblyNameTest()
        {
            var name = nameof(Gui) + ".exe";

            Assert.AreEqual(Setup.Register.ExeName, name);
        }
    }
}
