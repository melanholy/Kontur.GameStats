using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Kontur.GameStats.Server.ApiDatatypes;
using Kontur.GameStats.Server.Database;
using Kontur.GameStats.Server.Models;
using Kontur.GameStats.Server.Routes;
using Kontur.GameStats.Server.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Kontur.GameStats.Tests.Routes
{
    [TestClass]
    public class BestPlayersRouteTests
    {
        [TestInitialize]
        public void Setup()
        {
            EffortConnectionFactory.ResetDb();
        }

        [TestMethod]
        public void TestBestPlayersNormal()
        {
            var random = new Random();
            var killsTotal = new int[2];
            var deathsTotal = new int[2];

            using (var db = new ServerDatabase())
            {
                for (var i = 0; i < 15; i++)
                    for (var j = 0; j < 2; j++)
                    {
                        var deaths = random.Next(1, 15);
                        var kills = random.Next(0, 15);
                        killsTotal[j] += kills;
                        deathsTotal[j] += deaths;
                        var score = new PlayerScore
                        {
                            Deaths = deaths,
                            Kills = kills,
                            Name = j.ToString()
                        };
                        db.PlayerScores.Add(score);
                    }
                db.SaveChanges();
            }

            var request = new HttpRequest(HttpMethod.Get, Stream.Null);
            var urlArgs = new Dictionary<string, string> {{"entity", "best-players"}};
            var response = ReportsRoutes.ReportWithCount(urlArgs, request);
            var actual = JsonConvert.DeserializeObject<List<BestPlayerReport>>(response.Content);
            var expected = new[]
                {
                    new BestPlayerReport {KillToDeathRatio = (double) killsTotal[0]/deathsTotal[0], Name = "0"},
                    new BestPlayerReport {KillToDeathRatio = (double) killsTotal[1]/deathsTotal[1], Name = "1"}
                }
                .OrderByDescending(score => score.KillToDeathRatio)
                .ToList();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBestPlayersEmpty()
        {
            var request = new HttpRequest(HttpMethod.Get, Stream.Null);
            var urlArgs = new Dictionary<string, string> {{"entity", "best-players"}};
            var response = ReportsRoutes.ReportWithCount(urlArgs, request);
            var actual = JsonConvert.DeserializeObject<List<BestPlayerReport>>(response.Content);
            var expected = new List<BestPlayerReport>();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBestPlayersZeroDeaths()
        {
            using (var db = new ServerDatabase())
            {
                for (var i = 0; i < 15; i++)
                {
                    var score = new PlayerScore
                    {
                        Deaths = 0,
                        Kills = 10,
                        Name = "one"
                    };
                    db.PlayerScores.Add(score);
                }
                db.SaveChanges();
            }

            var request = new HttpRequest(HttpMethod.Get, Stream.Null);
            var urlArgs = new Dictionary<string, string> { { "entity", "best-players" } };
            var response = ReportsRoutes.ReportWithCount(urlArgs, request);
            var actual = JsonConvert.DeserializeObject<List<BestPlayerReport>>(response.Content);
            var expected = new List<BestPlayerReport>();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBestPlayersLessThanTenGames()
        {
            using (var db = new ServerDatabase())
            {
                for (var i = 0; i < 6; i++)
                {
                    var score = new PlayerScore
                    {
                        Deaths = 10,
                        Kills = 10,
                        Name = "one"
                    };
                    db.PlayerScores.Add(score);
                }
                db.SaveChanges();
            }

            var request = new HttpRequest(HttpMethod.Get, Stream.Null);
            var urlArgs = new Dictionary<string, string> { { "entity", "best-players" } };
            var response = ReportsRoutes.ReportWithCount(urlArgs, request);
            var actual = JsonConvert.DeserializeObject<List<BestPlayerReport>>(response.Content);
            var expected = new List<BestPlayerReport>();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
