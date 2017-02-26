using System.Collections.Generic;
using Kontur.GameStats.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kontur.GameStats.Tests.Infrastructure
{
    [TestClass]
    public class EnumerableEqualsTests
    {
        [TestMethod]
        public void TestNull()
        {
            var a = new List<int>();
            List<int> b = null;

            Assert.IsFalse(a.Equals(b, i => i));
        }

        [TestMethod]
        public void TestDifferentCount()
        {
            var a = new List<int>() {1, 2, 3};
            var b = new List<int>() {1, 2, 3, 4};

            Assert.IsFalse(a.Equals(b, i => i));
            Assert.IsFalse(b.Equals(a, i => i));
        }

        [TestMethod]
        public void TestEquals()
        {
            var a = new List<int>() {1, 2, 3, 4, 5};
            var b = new List<int>() {1, 4, 3, 2, 5};

            Assert.IsTrue(a.Equals(b, i => i));
            Assert.IsTrue(b.Equals(a, i => i));
            Assert.IsTrue(a.Equals(a, i => i));
        }

        [TestMethod]
        public void TestNotEquals()
        {
            var a = new List<int>() { 1, 2, 3, 4, 5 };
            var b = new List<int>() { 1, 6, 3, 2, 5 };

            Assert.IsFalse(a.Equals(b, i => i));
            Assert.IsFalse(b.Equals(a, i => i));
        }
    }
}
