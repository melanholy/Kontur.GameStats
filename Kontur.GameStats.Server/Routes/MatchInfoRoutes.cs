using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Kontur.GameStats.Server.Converters;
using Kontur.GameStats.Server.Database;
using Kontur.GameStats.Server.Models;
using Kontur.GameStats.Server.Routing;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.Routes
{
    public class MatchInfoRoutes : StatServerRouteProvider
    {
        private static readonly JsonDoubleConverter DoubleConverter =
            new JsonDoubleConverter();
        private static readonly JsonPlayerScoreConverter PlayerScoreConverter =
            new JsonPlayerScoreConverter();

        private static readonly Regex EndpointRegex =
            new Regex("(?:^(?:\\d{1,3}\\.){3}\\d{1,3}-\\d{1,5}$)|" +
                      "(?:^(?:(?:[a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\\-]*[a-zA-Z0-9])\\.)*" +
                      "(?:[A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\\-]*[A-Za-z0-9])-\\d{1,5}$)");

        public MatchInfoRoutes()
        {
            RegisterRoute(
                "/servers/<endpoint>/matches/<timestamp>", 
                new[] {HttpMethod.Put, HttpMethod.Get}, 
                MatchInfo);
        }

        private static HttpResponse AddMatch(Dictionary<string, string> urlArgs, HttpRequest request)
        {
            var endpoint = urlArgs["endpoint"];
            using (var db = new ServerDatabase())
            {
                var server = db.GameServers.FirstOrDefault(s => s.Endpoint == endpoint);
                if (server == null)
                    return new HttpResponse(HttpStatusCode.BadRequest);

                string data;
                using (var reader = new StreamReader(request.InputStream))
                    data = reader.ReadToEnd();

                GameMatch match;
                try
                {
                    match = JsonConvert.DeserializeObject<GameMatch>(
                        data,
                        PlayerScoreConverter);
                }
                catch (JsonReaderException)
                {
                    return new HttpResponse(HttpStatusCode.BadRequest);
                }

                match.Server = server;
                match.Timestamp = DateTime.Parse(urlArgs["timestamp"]).ToUniversalTime();
                match.TotalPlayers = match.Scoreboard.Count;
                for (var i = 1; i < match.Scoreboard.Count + 1; i++)
                    match.Scoreboard.ElementAt(i - 1).Place = i;

                db.GameMatches.Add(match);
                db.SaveChanges();
            }

            return new HttpResponse(HttpStatusCode.OK);
        }

        private static HttpResponse GetMatch(Dictionary<string, string> urlArgs)
        {
            var endpoint = urlArgs["endpoint"];
            var timestamp = DateTime.Parse(urlArgs["timestamp"]).ToUniversalTime();
            GameMatch match;
            using (var db = new ServerDatabase())
                match = db.GameMatches
                    .Include(m => m.Scoreboard)
                    .Include(m => m.Server)
                    .FirstOrDefault(m =>
                        m.Server.Endpoint == endpoint &&
                        m.Timestamp == timestamp);

            if (match == null)
                return new HttpResponse(HttpStatusCode.NotFound);

            match.Scoreboard = match.Scoreboard.OrderByDescending(score => score.Frags).ToList();
            return new HttpResponse(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(
                    match,
                    DoubleConverter));
        }
        
        public static HttpResponse MatchInfo(Dictionary<string, string> urlArgs, HttpRequest request)
        {
            var address = urlArgs["endpoint"];
            if (!EndpointRegex.IsMatch(address))
                return new HttpResponse(HttpStatusCode.BadRequest);

            if (request.Method == HttpMethod.Put)
                return AddMatch(urlArgs, request);

            return GetMatch(urlArgs);
        }
    }
}
