using System;
using System.Collections.Generic;
using System.IO;
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
    public class RecentMatchesRouteTests
    {
        [TestInitialize]
        public void Setup()
        {
            EffortConnectionFactory.ResetDb();
        }

        private static readonly GameServer testServer = new GameServer
            {
                Endpoint = "test.com",
                Name = "] My P3rfect Server [",
                GameModes = new GameMode[0]
            };

        private static GameMatch GetMatchWithTimestamp(DateTime timestamp)
        {
            return new GameMatch
            {
                GameMode = "DM",
                FragLimit = 0,
                Map = "Dust",
                Timestamp = timestamp,
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
        }

        [TestMethod]
        public void TestRecentMatches()
        {
            var urlArgs = new Dictionary<string, string>
            {
                {"entity", "recent-matches" }
            };
            var request = new HttpRequest(HttpMethod.Get, Stream.Null);
            
            var timestamp = DateTime.MinValue.AddDays(5);
            using (var db = new ServerDatabase())
            {
                for (var i = 0; i < 3; i++)
                {
                    db.GameMatches.Add(GetMatchWithTimestamp(timestamp));
                    timestamp = timestamp.AddDays(1);
                }
                db.SaveChanges();
            }

            var response = ReportsRoutes.ReportWithCount(urlArgs, request);
            var actualJson = JToken.Parse(response.Content);
            var expected =
                "[{\"server\":\"test.com\",\"timestamp\":\"0001-01-08T00:00:00Z\",\"results\":{" +
                "\"scoreboard\":[{\"name\":\"Vasya\",\"frags\":0,\"kills\":42,\"deaths\":0}]" +
                ",\"map\":\"Dust\",\"gameMode\":\"DM\",\"fragLimit\":0,\"timeLimit\":0,\"timeElapsed\":0.000000}}" +
                ",{\"server\":\"test.com\",\"timestamp\":\"0001-01-07T00:00:00Z\",\"results\":{" +
                "\"scoreboard\":[{\"name\":\"Vasya\",\"frags\":0,\"kills\":42,\"deaths\":0}]," +
                "\"map\":\"Dust\",\"gameMode\":\"DM\",\"fragLimit\":0,\"timeLimit\":0,\"timeElapsed\":0.000000}}" +
                ",{\"server\":\"test.com\",\"timestamp\":\"0001-01-06T00:00:00Z\",\"results\":{\"scoreboard\":" + 
                "[{\"name\":\"Vasya\",\"frags\":0,\"kills\":42,\"deaths\":0}],\"map\":\"Dust\",\"gameMode\":\"DM\"," +
                "\"fragLimit\":0,\"timeLimit\":0,\"timeElapsed\":0.000000}}]";
            var expectedJson = JToken.Parse(expected);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(JToken.DeepEquals(expectedJson, actualJson));
        }

        [TestMethod]
        public void TestRecentMatchesEmpty()
        {
            var urlArgs = new Dictionary<string, string> { {"entity", "recent-matches"} };
            var request = new HttpRequest(HttpMethod.Get, Stream.Null);

            var response = ReportsRoutes.ReportWithCount(urlArgs, request);
            var actualJson = JToken.Parse(response.Content);
            var expected = "[]";
            var expectedJson = JToken.Parse(expected);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(JToken.DeepEquals(expectedJson, actualJson));
        }
    }
}
