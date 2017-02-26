using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Kontur.GameStats.Server.Database;
using Kontur.GameStats.Server.Models;
using Kontur.GameStats.Server.Routes;
using Kontur.GameStats.Server.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Kontur.GameStats.Tests.Routes
{
    [TestClass]
    public class ServerInfoRoutesTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Effort.Provider.EffortProviderConfiguration.RegisterProvider();
        }

        [TestInitialize]
        public void Setup()
        {
            EffortProviderFactory.ResetDb();
        }

        [TestMethod]
        public void TestAddServer()
        {
            var urlArgs = new Dictionary<string, string> { {"endpoint", "test.com-8080"} };
            var expected = new GameServer
            {
                Endpoint = "test.com-8080",
                Name = "] My P3rfect Server [",
                GameModes = new[] {new GameMode {Name = "DM"}, new GameMode {Name = "TDM"}}
            };
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write("{\"name\": \"] My P3rfect Server [\",\"gameModes\": [ \"DM\", \"TDM\" ]}");
                writer.Flush();
                stream.Position = 0;
                var request = new HttpRequest("PUT", stream);
                var response = ServerInfoRoutes.ServerInfo(urlArgs, request);

                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
                using (var db = new ServerDatabase())
                {
                    var a = db.GameServers.ToList();
                    CollectionAssert.AreEqual(new[] {expected}, a);
                }
            }
        }

        [TestMethod]
        public void TestUpdateServer()
        {
            var urlArgs = new Dictionary<string, string> { { "endpoint", "test.com-8080" } };
            var original = new GameServer
            {
                Endpoint = "test.com-8080",
                Name = "Server",
                GameModes = new[] { new GameMode { Name = "DM" }, new GameMode { Name = "TDM" } }
            };
            var expected = new GameServer
            {
                Endpoint = "test.com-8080",
                Name = "Server",
                GameModes = new[] { new GameMode { Name = "TDM" }, new GameMode { Name = "TTM" } }
            };
            using (var stream = new MemoryStream())
            {
                using (var db = new ServerDatabase())
                {
                    db.GameServers.Add(original);
                    db.SaveChanges();
                }

                var writer = new StreamWriter(stream);
                writer.Write("{\"name\": \"Server\",\"gameModes\": [ \"TDM\", \"TTM\" ]}");
                writer.Flush();
                stream.Position = 0;
                var request = new HttpRequest("PUT", stream);
                var response = ServerInfoRoutes.ServerInfo(urlArgs, request);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            }

            using (var db = new ServerDatabase())
            {
                var a = db.GameServers.ToList();
                CollectionAssert.AreEqual(new[] { expected }, a);
            }
        }

        [TestMethod]
        public void TestGetServer()
        {
            var server = new GameServer
            {
                Endpoint = "test.com-8080",
                Name = "] My P3rfect Server [",
                GameModes = new[] { new GameMode { Name = "DM" } }
            };
            var urlArgs = new Dictionary<string, string> { { "endpoint", "test.com-8080" } };
            var request = new HttpRequest("GET", Stream.Null);
            var response = ServerInfoRoutes.ServerInfo(urlArgs, request);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            using (var db = new ServerDatabase())
            {
                db.GameServers.Add(server);
                db.SaveChanges();
            }
            var expected = "{\"name\":\"] My P3rfect Server [\",\"gameModes\":[\"DM\"]}";
            var expectedJson = JToken.Parse(expected);
            response = ServerInfoRoutes.ServerInfo(urlArgs, request);
            var actual = JToken.Parse(response.Content);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(JToken.DeepEquals(actual, expectedJson));
        }

        [TestMethod]
        public void TestInvalidHostname()
        {
            var request = new HttpRequest("GET", Stream.Null);
            var urlArgs = new Dictionary<string, string> { { "endpoint", "test.com" } };
            var response = ServerInfoRoutes.ServerInfo(urlArgs, request);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void TestGetAllServers()
        {
            var server1 = new GameServer
            {
                Endpoint = "test1.com-8080",
                Name = "1",
                GameModes = new [] {new GameMode { Name = "DM"} }
            };
            var server2 = new GameServer
            {
                Endpoint = "test2.com-8080",
                Name = "2",
                GameModes = new GameMode[0]
            };
            var urlArgs = new Dictionary<string, string>();
            var request = new HttpRequest("GET", Stream.Null);

            using (var db = new ServerDatabase())
            {
                db.GameServers.Add(server1);
                db.GameServers.Add(server2);
                db.SaveChanges();
            }

            var response = ServerInfoRoutes.GetAllServersInfo(urlArgs, request);
            var actual = JToken.Parse(response.Content);
            var expected =
                "[{\"endpoint\":\"test1.com-8080\",\"info\":{\"name\":\"1\",\"gameModes\":[\"DM\"]}},{\"endpoint\":\"test2.com-8080\",\"info\":{\"name\":\"2\",\"gameModes\":[]}}]";
            var expectedJson = JToken.Parse(expected);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(JToken.DeepEquals(expectedJson, actual));
        }

        [TestMethod]
        public void TestGetAllServersEmpty()
        {
            var urlArgs = new Dictionary<string, string>();
            var request = new HttpRequest("GET", Stream.Null);
            
            var response = ServerInfoRoutes.GetAllServersInfo(urlArgs, request);
            var actual = JToken.Parse(response.Content);
            var expected = "[]";
            var expectedJson = JToken.Parse(expected);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(JToken.DeepEquals(expectedJson, actual));
        }
    }
}
