using System.Collections.Generic;
using Kontur.GameStats.Server.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kontur.GameStats.Tests.Routing
{
    [TestClass]
    public class RouterTests
    {
        private class TestFactory : IRouteFactory
        {
            public static readonly Route TestRouteSimple = new Route("/test", new[] { "GET" },
                (dictionary, request) => null);
            public static readonly Route TestRouteWithVariables = new Route("/player/<name>/<age>", new[] { "GET" },
                (dictionary, request) => null);
            public static readonly Route TestRouteWithOptionalPath = new Route("/server[/optional]/needed[/another]", new[] { "GET" },
                (dictionary, request) => null);
            public static readonly Route TestRouteComplex = new Route("/path[/<optionalArg>]/<neededArg>/end", new[] { "GET" },
                (dictionary, request) => null);

            public IEnumerable<Route> GetRoutes()
            {
                return new[]
                {
                    TestRouteSimple,
                    TestRouteWithVariables,
                    TestRouteWithOptionalPath,
                    TestRouteComplex
                };
            }
        }

        private readonly Router router = new Router(new TestFactory());
        
        [TestMethod]
        public void TestRouteMatchSimple()
        {
            var match = router.Match("/notExistentRoute");
            Assert.IsNull(match);
            match = router.Match("/test1");
            Assert.IsNull(match);
            match = router.Match("/test/notExists");
            Assert.IsNull(match);

            match = router.Match("/test");
            Assert.AreSame(match.Route, TestFactory.TestRouteSimple);
        }

        [TestMethod]
        public void TestRouteMatchWithVariables()
        {
            var match = router.Match("/player/hero");
            Assert.IsNull(match);
            match = router.Match("/player/name/age/notExists");
            Assert.IsNull(match);

            match = router.Match("/player/name/age");
            Assert.AreSame(match.Route, TestFactory.TestRouteWithVariables);
            Assert.IsTrue(match.UrlArguments.ContainsKey("name"));
            Assert.IsTrue(match.UrlArguments.ContainsKey("age"));
            Assert.AreEqual(match.UrlArguments["name"], "name");
            Assert.AreEqual(match.UrlArguments["age"], "age");
        }

        [TestMethod]
        public void TestRouteMatchWithOptionalPath()
        {
            var match = router.Match("/server/another");
            Assert.IsNull(match);
            match = router.Match("/server/optional");
            Assert.IsNull(match);

            match = router.Match("/server/optional/needed");
            Assert.AreSame(match.Route, TestFactory.TestRouteWithOptionalPath);
            match = router.Match("/server/needed");
            Assert.AreSame(match.Route, TestFactory.TestRouteWithOptionalPath);
            match = router.Match("/server/optional/needed/another");
            Assert.AreSame(match.Route, TestFactory.TestRouteWithOptionalPath);
        }

        [TestMethod]
        public void TestRouteMatchComplex()
        {
            var match = router.Match("/path/needed/end");
            Assert.AreSame(match.Route, TestFactory.TestRouteComplex);
            Assert.IsTrue(match.UrlArguments.ContainsKey("neededArg"));
            Assert.IsFalse(match.UrlArguments.ContainsKey("optionalArg"));
            Assert.AreEqual(match.UrlArguments["neededArg"], "needed");

            match = router.Match("/path/optional/needed/end");
            Assert.AreSame(match.Route, TestFactory.TestRouteComplex);
            Assert.IsTrue(match.UrlArguments.ContainsKey("neededArg"));
            Assert.IsTrue(match.UrlArguments.ContainsKey("optionalArg"));
            Assert.AreEqual(match.UrlArguments["neededArg"], "needed");
            Assert.AreEqual(match.UrlArguments["optionalArg"], "optional");
        }
    }
}
