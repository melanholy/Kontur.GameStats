using System;
using System.Collections.Generic;
using System.IO;
using Kontur.GameStats.Server.Database;
using Kontur.GameStats.Server.Models;
using Kontur.GameStats.Server.Routes;
using Kontur.GameStats.Server.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Kontur.GameStats.Tests.Routes
{
    [TestClass]
    public class ReportsRoutesCountTest
    {
        [TestInitialize]
        public void Setup()
        {
            EffortConnectionFactory.ResetDb();
            var server = new GameServer
            {
                Endpoint = "test.com",
                Name = "] My P3rfect Server [",
                GameModes = new GameMode[0]
            };
            using (var db = new ServerDatabase())
            {
                db.GameServers.Add(server);
                for (var i = 0; i < 90; i++)
                {
                    var match = new GameMatch
                    {
                        GameMode = "DM",
                        FragLimit = 0,
                        Map = "Dust",
                        Timestamp = DateTime.MaxValue,
                        TimeLimit = 0,
                        TotalPlayers = 1,
                        Scoreboard = new List<PlayerScore>
                        {
                            new PlayerScore
                            {
                                Deaths = 0,
                                Frags = 0,
                                Kills = 42,
                                Name = "Vasya",
                                Place = 1
                            }
                        },
                        Server = server
                    };
                    db.GameMatches.Add(match);
                }
                db.SaveChanges();
            }
        }

        private static void TestCountOnRecentMatches(int count, int expected)
        {
            var urlArgs = new Dictionary<string, string>
            {
                {"entity", "recent-matches"},
                {"count", count.ToString()}
            };
            var request = new HttpRequest(HttpMethod.Get, Stream.Null);
            var response = ReportsRoutes.ReportWithCount(urlArgs, request);
            var array = JArray.Parse(response.Content);

            Assert.AreEqual(expected, array.Count);
        }

        [TestMethod]
        public void TestCountNegativeAndZero()
        {
            TestCountOnRecentMatches(-1000, 0);
            TestCountOnRecentMatches(0, 0);
        }

        [TestMethod]
        public void TestCountNormal()
        {
            TestCountOnRecentMatches(39, 39);
            TestCountOnRecentMatches(1, 1);
            TestCountOnRecentMatches(50, 50);
        }

        [TestMethod]
        public void TestCountAboveFifty()
        {
            TestCountOnRecentMatches(1000, 50);
            TestCountOnRecentMatches(51, 50);
        }
    }
}
