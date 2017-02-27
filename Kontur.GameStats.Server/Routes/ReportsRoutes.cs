using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using Kontur.GameStats.Server.ApiDatatypes;
using Kontur.GameStats.Server.Converters;
using Kontur.GameStats.Server.Database;
using Kontur.GameStats.Server.Models;
using Kontur.GameStats.Server.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kontur.GameStats.Server.Routes
{
    public class ReportsRoutes : StatServerRouteProvider
    {
        private const int DefaultCountValue = 5;
        private const int MaxCountValue = 50;

        private static readonly JsonDoubleConverter DoubleConverter =
            new JsonDoubleConverter();
        private static readonly JsonDatetimeConverter DatetimeConverter =
            new JsonDatetimeConverter();
        private static readonly JsonGameMatchConverter GameMatchConverter =
            new JsonGameMatchConverter();

        public ReportsRoutes()
        {
            RegisterRoute("/reports/<entity>[/<count>]", new[] {HttpMethod.Get}, ReportWithCount);
        }

        private static IEnumerable<GameMatch> GetRecentMatchesWithConstraint(
            Func<IQueryable<GameMatch>, IQueryable<GameMatch>> constraint)
        {
            using (var db = new ServerDatabase())
            {
                var recentMatches = constraint(db.GameMatches
                    .Include(match => match.Scoreboard)
                    .Include(match => match.Server)
                    .OrderByDescending(match => match.Timestamp));

                return recentMatches.ToList().Select(match =>
                {
                    match.Scoreboard = match.Scoreboard.OrderByDescending(score => score.Frags).ToList();
                    return match;
                });
            }
        }

        private static IEnumerable<BestPlayerReport> GetBestPlayersWithConstraint(
            Func<IQueryable<BestPlayerReport>, IQueryable<BestPlayerReport>> constraint)
        {
            using (var db = new ServerDatabase())
            {
                var bestPlayers = constraint(db.PlayerScores
                    .GroupBy(score => score.Name)
                    .Where(group => group.Count() >= 10)
                    .Select(group => new
                    {
                        Kills = group.Sum(score => score.Kills),
                        Deaths = group.Sum(score => score.Deaths),
                        Name = group.Key
                    })
                    .Where(x => x.Deaths > 0)
                    .Select(x => new BestPlayerReport
                    {
                        // другого способа преобразовать в double не нашлось :(
                        KillToDeathRatio = (x.Kills + 0.5 - 0.5) / x.Deaths,
                        Name = x.Name
                    })
                    .OrderByDescending(report => report.KillToDeathRatio));

                return bestPlayers.ToList();
            }
        }

        private static IEnumerable<PopularServerReport> GetPopularServersWithConstraint(
            Func<IQueryable<PopularServerReport>, IQueryable<PopularServerReport>> constraint)
        {
            using (var db = new ServerDatabase())
            {
                var popularServers = constraint(db.GameMatches
                    .GroupBy(match => new
                    {
                        match.Timestamp.Year,
                        match.Timestamp.Month,
                        match.Timestamp.Day,
                        match.Server.Endpoint,
                        match.Server.Name
                    })
                    .Select(group => new
                    {
                        Count = group.Count(),
                        group.Key.Name,
                        group.Key.Endpoint
                    })
                    .GroupBy(x => new { x.Name, x.Endpoint })
                    .Select(group => new PopularServerReport
                    {
                        AverageMatchesPerDay = group.Average(x => x.Count),
                        Name = group.Key.Name,
                        ServerAddress = group.Key.Endpoint
                    }))
                    .OrderByDescending(report => report.AverageMatchesPerDay);

                return popularServers.ToList();
            }
        }

        private static Func<IQueryable<T>, IQueryable<T>> GetCountConstraint<T>(int count)
        {
            return collection => collection.Take(count);
        }
        
        public static HttpResponse ReportWithCount(Dictionary<string, string> urlArgs, HttpRequest request)
        {
            int count;
            if (urlArgs.ContainsKey("count"))
            {
                if (!int.TryParse(urlArgs["count"], out count))
                    return new HttpResponse(HttpStatusCode.BadRequest);

                if (count < 0)
                    count = 0;
                else if (count > MaxCountValue)
                    count = MaxCountValue;
            }
            else
                count = DefaultCountValue;

            if (count <= 0)
                return new HttpResponse(HttpStatusCode.OK, new JArray().ToString());

            string responseContent;
            switch (urlArgs["entity"])
            {
                case "recent-matches":
                    {
                        var countConstraint = GetCountConstraint<GameMatch>(count);
                        var matches = GetRecentMatchesWithConstraint(countConstraint);
                        responseContent = JsonConvert.SerializeObject(
                            matches,
                            DoubleConverter,
                            DatetimeConverter,
                            GameMatchConverter);
                        break;
                    }
                case "best-players":
                    {
                        var countConstraint = GetCountConstraint<BestPlayerReport>(count);
                        var players = GetBestPlayersWithConstraint(countConstraint);
                        responseContent = JsonConvert.SerializeObject(players, DoubleConverter);
                        break;
                    }
                case "popular-servers":
                    {
                        var countConstraint = GetCountConstraint<PopularServerReport>(count);
                        var servers = GetPopularServersWithConstraint(countConstraint);
                        responseContent = JsonConvert.SerializeObject(servers, DoubleConverter);
                        break;
                    }
                default:
                    return new HttpResponse(HttpStatusCode.NotFound);
            }

            return new HttpResponse(HttpStatusCode.OK, responseContent);
        }
    }
}