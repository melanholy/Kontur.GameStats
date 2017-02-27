using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text.RegularExpressions;
using Kontur.GameStats.Server.ApiDatatypes;
using Kontur.GameStats.Server.Converters;
using Kontur.GameStats.Server.Database;
using Kontur.GameStats.Server.Models;
using Kontur.GameStats.Server.Routing;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.Routes
{
    public class StatsRoutes : StatServerRouteProvider
    {
        private static readonly JsonDoubleConverter DoubleConverter =
            new JsonDoubleConverter();
        private static readonly JsonDatetimeConverter DatetimeConverter =
            new JsonDatetimeConverter();
        private static readonly Regex EndpointRegex =
            new Regex("(?:^(?:\\d{1,3}\\.){3}\\d{1,3}-\\d{1,5}$)|" +
                      "(?:^(?:(?:[a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\\-]*[a-zA-Z0-9])\\.)*" +
                      "(?:[A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\\-]*[A-Za-z0-9])-\\d{1,5}$)");

        public StatsRoutes()
        {
            RegisterRoute("/servers/<endpoint>/stats", new []{HttpMethod.Get}, GetServerStatsByEndpoint);
            RegisterRoute("/players/<name>/stats", new []{HttpMethod.Get}, GetPlayerStatsByName);
        }

        private static MatchesStats GetServerStatsByField(Expression<Func<GameMatch, bool>> selector)
        {
            using (var db = new ServerDatabase())
            {
                var stats = new MatchesStats();
                if (!db.GameMatches.Any(selector))
                {
                    stats.Top5GameModes = new List<string>();
                    stats.Top5Maps = new List<string>();
                    return stats;
                }
                   
                var generalStats = db.GameMatches
                    .Where(selector)
                    .Include(match => match.Scoreboard)
                    .GroupBy(match => 1)
                    .Select(group => new
                    {
                        TotalMatchesPlayed = group.Count(),
                        MaximumPopulation = group.Max(match => match.TotalPlayers),
                        AveragePopulation = group.Average(match => match.TotalPlayers)
                    })
                    .Single();
                stats.AveragePopulation = generalStats.AveragePopulation;
                stats.MaximumPopulation = generalStats.MaximumPopulation;
                stats.TotalMatchesPlayed = generalStats.TotalMatchesPlayed;

                var matchesPerDay = db.GameMatches
                    .Where(selector)
                    .GroupBy(match => new
                    {
                        match.Timestamp.Year,
                        match.Timestamp.Month,
                        match.Timestamp.Day
                    })
                    .Select(group => group.Count())
                    .GroupBy(count => 1)
                    .Select(group => new {Max = group.Max(), Average = group.Average()})
                    .Single();
                stats.AverageMatchesPerDay = matchesPerDay.Average;
                stats.MaximumMatchesPerDay = matchesPerDay.Max;

                stats.Top5Maps = db.GameMatches
                    .Where(selector)
                    .GroupBy(match => match.Map)
                    .OrderByDescending(group => group.Count())
                    .Select(group => group.Key)
                    .Take(5)
                    .ToList();

                stats.Top5GameModes = db.GameMatches
                    .Where(selector)
                    .GroupBy(match => match.GameMode)
                    .OrderByDescending(group => group.Count())
                    .Select(group => group.Key)
                    .Take(5)
                    .ToList();

                return stats;
            }
        }
        
        public static HttpResponse GetServerStatsByEndpoint(Dictionary<string, string> urlArgs, HttpRequest request)
        {
            var address = urlArgs["endpoint"];
            if (!EndpointRegex.IsMatch(address))
                return new HttpResponse(HttpStatusCode.BadRequest);

            var stats = GetServerStatsByField(match => match.Server.Endpoint == address);

            return new HttpResponse(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(stats, DoubleConverter));
        }

        private static PlayerStats GetPlayerStatsByField(Expression<Func<PlayerScore, bool>> selector)
        {
            using (var db = new ServerDatabase())
            {
                var stats = new PlayerStats();
                var count = db.PlayerScores.Count(selector);
                if (count == 0)
                    return stats;

                stats.TotalMatchesPlayed = count;

                var servers = db.PlayerScores
                    .Where(selector)
                    .Include(score => score.Match)
                    .GroupBy(score => score.Match.Server.Endpoint)
                    .Select(group => new {Count = group.Count(), Server = group.Key})
                    .OrderByDescending(x => x.Count);
                stats.FavoriteServer = servers.First().Server;
                stats.UniqueServers = servers.Count();

                stats.FavoriteGameMode = db.PlayerScores
                    .Where(selector)
                    .Include(score => score.Match)
                    .GroupBy(score => score.Match.GameMode)
                    .Select(group => new {Mode = group.Key, Count = group.Count()})
                    .OrderByDescending(x => x.Count)
                    .First()
                    .Mode;

                var matchesPerDay = db.PlayerScores
                    .Where(selector)
                    .Include(score => score.Match)
                    .GroupBy(score => new
                    {
                        score.Match.Timestamp.Year,
                        score.Match.Timestamp.Month,
                        score.Match.Timestamp.Day
                    })
                    .Select(group => group.Count())
                    .GroupBy(x => 1)
                    .Select(group => new {Max = group.Max(), Average = group.Average()})
                    .Single();
                stats.MaximumMatchesPerDay = matchesPerDay.Max;
                stats.AverageMatchesPerDay = matchesPerDay.Average;

                var generalStats = db.PlayerScores
                    .Where(selector)
                    .GroupBy(score => 1)
                    .Select(group => new
                    {
                        Kills = group.Sum(score => score.Kills),
                        Deaths = group.Sum(score => score.Deaths),
                        AverageScoreboardPercent = group.Average(score =>
                            score.Match.TotalPlayers != 1
                                ? (score.Match.TotalPlayers - score.Place)*100/(score.Match.TotalPlayers - 1)
                                : 100),
                        LastTimePlayed = group.Max(x => x.Match.Timestamp),
                        MatchesWon = group.Count(score => score.Place == 1)
                    })
                    .Single();
                stats.KillToDeathRatio = generalStats.Deaths == 0
                    ? 0
                    : (double) generalStats.Kills/generalStats.Deaths;
                stats.AverageScoreboardPercent = generalStats.AverageScoreboardPercent;
                stats.LastMatchPlayed = generalStats.LastTimePlayed;
                stats.TotalMatchesWon = generalStats.MatchesWon;

                return stats;
            }
        }
        
        public static HttpResponse GetPlayerStatsByName(Dictionary<string, string> urlArgs, HttpRequest request)
        {
            var name = urlArgs["name"].ToLower();
            var stats = GetPlayerStatsByField(score => score.Name.ToLower() == name);

            return new HttpResponse(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(
                    stats,
                    DoubleConverter,
                    DatetimeConverter));
        }
    }
}