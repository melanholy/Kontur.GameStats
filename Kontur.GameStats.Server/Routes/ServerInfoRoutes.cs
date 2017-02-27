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
    public class ServerInfoRoutes : StatServerRouteProvider
    {
        private static readonly JsonGameModeCollectionConverter GameModeCollectionConverter =
            new JsonGameModeCollectionConverter();

        private static readonly JsonGameServerConverter GameServerConverter =
            new JsonGameServerConverter();

        private static readonly Regex EndpointRegex =
            new Regex("(?:^(?:\\d{1,3}\\.){3}\\d{1,3}-\\d{1,5}$)|" +
                      "(?:^(?:(?:[a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\\-]*[a-zA-Z0-9])\\.)*" +
                      "(?:[A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\\-]*[A-Za-z0-9])-\\d{1,5}$)");

        public ServerInfoRoutes()
        {
            RegisterRoute("/servers/info", new[] {HttpMethod.Get}, GetAllServersInfo);
            RegisterRoute("/servers/<endpoint>/info", new[] {HttpMethod.Put, HttpMethod.Get}, ServerInfo);
        }

        private static HttpResponse GetServerInfo(Dictionary<string, string> urlArgs)
        {
            var address = urlArgs["endpoint"];
            using (var db = new ServerDatabase())
            {
                var serverInfo = db.GameServers
                    .Where(server => server.Endpoint == address)
                    .Include(x => x.GameModes)
                    .SingleOrDefault();

                if (serverInfo == null)
                    return new HttpResponse(HttpStatusCode.NotFound);

                return new HttpResponse(
                    HttpStatusCode.OK,
                    JsonConvert.SerializeObject(
                        serverInfo,
                        GameModeCollectionConverter));
            }
        }

        private static HttpResponse AddOrUpdateServerInfo(Dictionary<string, string> urlArgs,
            HttpRequest request)
        {
            string data;
            using (var reader = new StreamReader(request.InputStream))
                data = reader.ReadToEnd();

            GameServer server;
            try
            {
                server = JsonConvert.DeserializeObject<GameServer>(
                    data,
                    GameModeCollectionConverter);
            }
            catch (JsonReaderException)
            {
                return new HttpResponse(HttpStatusCode.BadRequest);
            }
            server.Endpoint = urlArgs["endpoint"];

            using (var db = new ServerDatabase())
            {
                var existingServer = db.GameServers
                    .Include(s => s.GameModes)
                    .SingleOrDefault(x => x.Endpoint == server.Endpoint);

                if (existingServer == null)
                    db.GameServers.Add(server);
                else
                {
                    server.Id = existingServer.Id;
                    db.GameModes.RemoveRange(existingServer.GameModes);
                    db.Entry(existingServer).CurrentValues.SetValues(server);
                    existingServer.GameModes = server.GameModes;
                }
                db.SaveChanges();
            }

            return new HttpResponse(HttpStatusCode.OK);
        }
        
        public static HttpResponse ServerInfo(Dictionary<string, string> urlArgs, HttpRequest request)
        {
            var address = urlArgs["endpoint"];
            if (!EndpointRegex.IsMatch(address))
                return new HttpResponse(HttpStatusCode.BadRequest);

            if (request.Method == HttpMethod.Put)
                return AddOrUpdateServerInfo(urlArgs, request);

            return GetServerInfo(urlArgs);
        }

        
        public static HttpResponse GetAllServersInfo(Dictionary<string, string> urlArgs, HttpRequest request)
        {
            using (var db = new ServerDatabase())
            {
                var servers = db.GameServers
                    .Include(x => x.GameModes);

                return new HttpResponse(
                    HttpStatusCode.OK,
                    JsonConvert.SerializeObject(
                        servers,
                        GameModeCollectionConverter,
                        GameServerConverter));
            }
        }
    }
}