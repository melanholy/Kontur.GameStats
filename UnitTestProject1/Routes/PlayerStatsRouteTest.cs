using System;
using System.Collections.Generic;
using System.IO;
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
    public class PlayerStatsRouteTest
    {
        [TestInitialize]
        public void Setup()
        {
            EffortProviderFactory.ResetDb();
        }

        [TestMethod]
        public void PlayerStatsNormal()
        {
            var server1 = new GameServer
            {
                Endpoint = "test1.com",
                GameModes = new List<GameMode>(),
                Name = "test1"
            };
            var server2 = new GameServer
            {
                Endpoint = "test2.com",
                GameModes = new List<GameMode>(),
                Name = "test2"
            };
            var date = DateTime.MinValue;
            var matches = new List<GameMatch>
            {
                new GameMatch
                {
                    GameMode = "DM", Map = "Dust", Timestamp = date,
                    Server = server1, TotalPlayers = 3
                },
                new GameMatch
                {
                    GameMode = "DM", Map = "Dust", Timestamp = date.AddDays(1),
                    Server = server2, TotalPlayers = 3
                },
                new GameMatch
                {
                    GameMode = "TDM", Map = "Mirage", Timestamp = date,
                    Server = server2, TotalPlayers = 3
                },
            };
            matches[0].Scoreboard = new List<PlayerScore>
            {
                new PlayerScore {Deaths = 4, Kills = 5, Match = matches[0], Name = "one", Place = 1},
                new PlayerScore {Deaths = 5, Kills = 4, Match = matches[0], Name = "two", Place = 2},
                new PlayerScore {Deaths = 6, Kills = 3, Match = matches[0], Name = "three", Place = 3}
            };
            matches[1].Scoreboard = new List<PlayerScore>
            {
                new PlayerScore {Deaths = 40, Kills = 5, Match = matches[1], Name = "two", Place = 1},
                new PlayerScore {Deaths = 5, Kills = 52, Match = matches[1], Name = "one", Place = 2},
                new PlayerScore {Deaths = 4, Kills = 12, Match = matches[1], Name = "three", Place = 3}
            };
            matches[2].Scoreboard = new List<PlayerScore>
            {
                new PlayerScore {Deaths = 1, Kills = 0, Match = matches[2], Name = "three", Place = 1},
                new PlayerScore {Deaths = 10, Kills = 22, Match = matches[2], Name = "two", Place = 2},
                new PlayerScore {Deaths = 47, Kills = 14, Match = matches[2], Name = "one", Place = 3}
            };

            using (var db = new ServerDatabase())
            {
                db.GameServers.Add(server1);
                db.GameServers.Add(server2);
                db.GameMatches.AddRange(matches);
                db.SaveChanges();
            }

            var urlArgs = new Dictionary<string, string> { {"name", "two"} };
            var request = new HttpRequest("GET", Stream.Null);
            var response = StatsRoutes.GetPlayerStatsByName(urlArgs, request);
            var expected = new PlayerStats
            {
                AverageMatchesPerDay = 1.5, AverageScoreboardPercent = (double)200/3,
                FavoriteGameMode = "DM", FavoriteServer = "test2.com",
                KillToDeathRatio = (double)(22 + 5 + 4) / (10 + 40 + 5),
                LastMatchPlayed = date.AddDays(1), MaximumMatchesPerDay = 2,
                TotalMatchesPlayed = 3, TotalMatchesWon = 1, UniqueServers = 2
            };
            var actual = JsonConvert.DeserializeObject<PlayerStats>(response.Content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestPlayerStatsEmpty()
        {
            var urlArgs = new Dictionary<string, string> { { "name", "two" } };
            var request = new HttpRequest("GET", Stream.Null);
            var response = StatsRoutes.GetPlayerStatsByName(urlArgs, request);
            var expected = new PlayerStats
            {
                LastMatchPlayed = DateTime.MinValue
            };
            var actual = JsonConvert.DeserializeObject<PlayerStats>(response.Content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(expected, actual);
        }
    }
}
