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
    public class ServerStatsRouteTests
    {
        [TestInitialize]
        public void Setup()
        {
            EffortConnectionFactory.ResetDb();
        }

        [TestMethod]
        public void TestMatchesStatsNormal()
        {
            var server = new GameServer
                {
                    Endpoint = "test.com-8080",
                    GameModes = new List<GameMode>(),
                    Name = "test"
                };
            var expected = new MatchesStats();
            var modes = new[] {"DM", "TDM", "DE", "DB", "KL", "MN"};
            var modesFrequency = new Dictionary<string, int>();
            var maps = new[] {"Dust", "Mirage", "Nuke", "Assault", "Mansion"};
            var mapsFrequency = new Dictionary<string, int>();
            var daysFrequency = new Dictionary<DateTime, int>();
            var timestamp = DateTime.Now;
            var rand = new Random();
            using (var db = new ServerDatabase())
            {
                db.GameServers.Add(server);
                db.SaveChanges();
                
                var count = rand.Next(99, 170);
                for (var j = 0; j < count; j++)
                {
                    var timespan = new TimeSpan(rand.Next(0, 17), 0, 0, 0);
                    var day = timestamp.Add(timespan);
                    var mode = modes[rand.Next(0, modes.Length - 1)];
                    var map = maps[rand.Next(0, maps.Length - 1)];
                    var population = rand.Next(1, 50);
                    var match = new GameMatch
                    {
                        Timestamp = day, Server = server, GameMode = mode,
                        Map = map, TotalPlayers = population
                    };
                    db.GameMatches.Add(match);

                    if (modesFrequency.ContainsKey(mode))
                        modesFrequency[mode]++;
                    else
                        modesFrequency[mode] = 1;
                    if (mapsFrequency.ContainsKey(map))
                        mapsFrequency[map]++;
                    else
                        mapsFrequency[map] = 1;
                    if (daysFrequency.ContainsKey(day))
                        daysFrequency[day]++;
                    else
                        daysFrequency[day] = 1;
                    if (population > expected.MaximumPopulation)
                        expected.MaximumPopulation = population;
                    expected.AveragePopulation += population;
                }
                expected.AveragePopulation /= count;
                expected.TotalMatchesPlayed = count;
                db.SaveChanges();
            }
            expected.AverageMatchesPerDay = (double)daysFrequency.Sum(x => x.Value) / daysFrequency.Count;
            expected.MaximumMatchesPerDay = daysFrequency.Max(x => x.Value);
            expected.Top5GameModes = modesFrequency
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key);
            expected.Top5Maps = mapsFrequency
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key);
            
            var request = new HttpRequest(HttpMethod.Get, Stream.Null);
            var urlArgs = new Dictionary<string, string> { {"endpoint", "test.com-8080"} };
            var response = StatsRoutes.GetServerStatsByEndpoint(urlArgs, request);
            var actual = JsonConvert.DeserializeObject<MatchesStats>(response.Content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void TestMatchesStatsEmpty()
        {
            var request = new HttpRequest(HttpMethod.Get, Stream.Null);
            var urlArgs = new Dictionary<string, string> { { "endpoint", "test.com-8080" } };
            var response = StatsRoutes.GetServerStatsByEndpoint(urlArgs, request);
            var actual = JsonConvert.DeserializeObject<MatchesStats>(response.Content);
            var expected = new MatchesStats
            {
                Top5Maps = new List<string>(),
                Top5GameModes = new List<string>()
            };

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void TestMatchesStatsInvalidHostname()
        {
            var request = new HttpRequest(HttpMethod.Get, Stream.Null);
            var urlArgs = new Dictionary<string, string> { { "endpoint", "test.com" } };
            var response = StatsRoutes.GetServerStatsByEndpoint(urlArgs, request);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
