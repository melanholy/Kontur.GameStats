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
    public class PopularServersRouteTests
    {
        [TestInitialize]
        public void Setup()
        {
            EffortProviderFactory.ResetDb();
        }

        [TestMethod]
        public void TestPopularServers()
        {
            var servers = new List<GameServer>();
            for (var i = 1; i < 4; i++)
                servers.Add(new GameServer
                {
                    Endpoint = $"test{i}.com",
                    GameModes = new List<GameMode>(),
                    Name = $"test{i}"
                });
            var averageMatchesPerDay = new List<double>();
            var timestamp = DateTime.Now;
            var rand = new Random();
            foreach (var server in servers)
                using (var db = new ServerDatabase())
                {
                    db.GameServers.Add(server);
                    db.SaveChanges();
                    var days = new Dictionary<DateTime, int>();
                    for (var j = 0; j < rand.Next(100, 200); j++)
                    {
                        var timespan = new TimeSpan(rand.Next(0, 17), 0, 0, 0);
                        var day = timestamp.Add(timespan);
                        var match = new GameMatch {Timestamp = day, Server = server};
                        
                        db.GameMatches.Add(match);
                        if (days.ContainsKey(day))
                            days[day]++;
                        else
                            days[day] = 1;
                    }
                    averageMatchesPerDay.Add((double)days.Sum(x => x.Value) / days.Count);
                    db.SaveChanges();
                }

            var request = new HttpRequest("GET", Stream.Null);
            var urlArgs = new Dictionary<string, string> { {"entity", "popular-servers"} };
            var response = ReportsRoutes.ReportWithCount(urlArgs, request);
            var actual = JsonConvert.DeserializeObject<List<PopularServerReport>>(response.Content);
            var expected = servers.Select((t, i) => new PopularServerReport
            {
                AverageMatchesPerDay = averageMatchesPerDay[i],
                Name = t.Name,
                ServerAddress = t.Endpoint
            })
            .OrderByDescending(x => x.AverageMatchesPerDay)
            .ToList();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestPopularServersEmpty()
        {
            var request = new HttpRequest("GET", Stream.Null);
            var urlArgs = new Dictionary<string, string> { { "entity", "popular-servers" } };
            var response = ReportsRoutes.ReportWithCount(urlArgs, request);
            var actual = JsonConvert.DeserializeObject<List<PopularServerReport>>(response.Content);
            var expected = new List<PopularServerReport>();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
