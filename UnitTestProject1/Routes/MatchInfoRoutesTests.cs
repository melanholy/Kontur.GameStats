using System;
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
    public class MatchInfoRoutesTests
    {
        private static readonly GameServer testServer = new GameServer
            {
                Endpoint = "test.com-8080",
                Name = "] My P3rfect Server [",
                GameModes = new GameMode[0]
            };
        private static readonly GameMatch testMatch = new GameMatch
        {
            GameMode = "DM",
            FragLimit = 0,
            Map = "Dust",
            Timestamp = DateTime.MinValue.ToUniversalTime(),
            TimeLimit = 0,
            TotalPlayers = 1,
            Scoreboard = new List<PlayerScore> {new PlayerScore
                {
                    Deaths = 0,
                    Frags = 0,
                    Kills = 42,
                    Name = "Vasya",
                    Place = 1
                } },
            Server = testServer
        };

        [TestInitialize]
        public void Setup()
        {
            EffortConnectionFactory.ResetDb();
        }

        [TestMethod]
        public void TestGetMatch()
        {
            var server = testServer;
            var match = testMatch;
            var urlArgs = new Dictionary<string, string>
            {
                { "endpoint", "test.com-8080" },
                { "timestamp", "0001-01-01T00:00:00Z" }
            };
            var request = new HttpRequest(HttpMethod.Get, Stream.Null);
            var response = MatchInfoRoutes.MatchInfo(urlArgs, request);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            using (var db = new ServerDatabase())
            {
                db.GameServers.Add(server);
                db.GameMatches.Add(match);
                db.SaveChanges();
            }

            var expected = "{\"scoreboard\":[{\"name\":\"Vasya\",\"frags\":0,\"kills\":42,\"deaths\":0}],\"map\":\"Dust\",\"gameMode\":\"DM\",\"fragLimit\":0,\"timeLimit\":0,\"timeElapsed\":0.000000}";
            var expectedJson = JToken.Parse(expected);
            response = MatchInfoRoutes.MatchInfo(urlArgs, request);
            var actualJson = JToken.Parse(response.Content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(JToken.DeepEquals(actualJson, expectedJson));
        }

        [TestMethod]
        public void TestAddMatch()
        {
            var urlArgs = new Dictionary<string, string>
            {
                { "endpoint", "test.com-8080" },
                { "timestamp", "0001-01-01T00:00:00Z" }
            };
            var server = testServer;
            var expected = testMatch;
            var request = new HttpRequest(HttpMethod.Put, Stream.Null);
            var response = MatchInfoRoutes.MatchInfo(urlArgs, request);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            using (var stream = new MemoryStream())
            {
                using (var db = new ServerDatabase())
                {
                    db.GameServers.Add(server);
                    db.SaveChanges();
                }
                var writer = new StreamWriter(stream);
                writer.Write(@"{""scoreboard"":[
                                    {""name"":""Vasya"",""frags"":0,""kills"":42,""deaths"":0}
                                ], ""map"":""Dust"",""gameMode"":""DM"",""fragLimit"":0,
                                ""timeLimit"":0,""timeElapsed"":0.000000}");
                writer.Flush();
                stream.Position = 0;
                request = new HttpRequest(HttpMethod.Put, stream);
                response = MatchInfoRoutes.MatchInfo(urlArgs, request);
            }

            using (var db = new ServerDatabase())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
                var a = db.GameMatches.ToList();
                CollectionAssert.AreEqual(new[] { expected }, a);
            }
        }

        [TestMethod]
        public void TestInvalidHostname()
        {
            var request = new HttpRequest(HttpMethod.Get, Stream.Null);
            var urlArgs = new Dictionary<string, string> { { "endpoint", "test.com" } };
            var response = MatchInfoRoutes.MatchInfo(urlArgs, request);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
