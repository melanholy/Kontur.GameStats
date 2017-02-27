using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using Kontur.GameStats.Server.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Kontur.GameStats.Tests.Routing
{
    [TestClass]
    public class RouteTests
    {
        [TestMethod]
        public void TestRouteHandleNormal()
        {
            var expectedResponse = new HttpResponse(HttpStatusCode.OK);
            var args = new Dictionary<string, string>();
            var getRequest = new HttpRequest(HttpMethod.Get, Stream.Null);

            var mockHandler =
                MockRepository.GenerateStub<Func<Dictionary<string, string>, HttpRequest, HttpResponse>>();
            mockHandler
                .Stub(x => x(Arg<Dictionary<string, string>>.Is.Same(args), Arg<HttpRequest>.Is.Same(getRequest)))
                .Return(expectedResponse);

            var route = new Route("/test", new[] { HttpMethod.Get, HttpMethod.Put }, mockHandler);
            var actualResponse = route.Handle(args, getRequest);
            
            Assert.AreSame(expectedResponse, actualResponse);
            mockHandler.AssertWasCalled(x => 
                x(Arg<Dictionary<string, string>>.Is.Same(args), Arg<HttpRequest>.Is.Same(getRequest)));
            
            var putRequest = new HttpRequest(HttpMethod.Put, Stream.Null);
            mockHandler
                .Stub(x => x(Arg<Dictionary<string, string>>.Is.Same(args), Arg<HttpRequest>.Is.Same(putRequest)))
                .Return(expectedResponse);
            actualResponse = route.Handle(args, putRequest);

            Assert.AreSame(expectedResponse, actualResponse);
            mockHandler.AssertWasCalled(x =>
                x(Arg<Dictionary<string, string>>.Is.Same(args), Arg<HttpRequest>.Is.Same(putRequest)));
        }

        [TestMethod]
        public void TestRouteHandleMethodNotAllowed()
        {
            var mockHandler =
                MockRepository.GenerateStub<Func<Dictionary<string, string>, HttpRequest, HttpResponse>>();
            var route = new Route("/", new []{HttpMethod.Get, HttpMethod.Put}, mockHandler);

            var postRequest = new HttpRequest(HttpMethod.Post, Stream.Null);

            Assert.AreEqual(route.Handle(null, postRequest).StatusCode, HttpStatusCode.MethodNotAllowed);
            mockHandler.AssertWasNotCalled(x => 
                x(Arg<Dictionary<string, string>>.Is.Anything, Arg<HttpRequest>.Is.Anything));
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestIncorrectPath()
        {
            MyAssert.Throws<ArgumentException>(() => new Route("test", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/test>", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/<test", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/[test", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/<test]", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/test]", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/test/[test]", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/<test>[/test", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/test[/<test]", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/test[/test>]", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/test[/<test]>", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/test/", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/<>", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/test[/]", null, null));
            MyAssert.Throws<ArgumentException>(() => new Route("/test[/<>]", null, null));
        }
    }
}
