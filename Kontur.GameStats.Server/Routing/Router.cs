using System.Collections.Generic;
using System.Linq;

namespace Kontur.GameStats.Server.Routing
{
    public class RouteMatchResult
    {
        public Dictionary<string, string> UrlArguments { get; }
        public Route Route { get; }

        public RouteMatchResult(Route route, Dictionary<string, string> urlArguments)
        {
            UrlArguments = urlArguments;
            Route = route;
        }
    }

    public class Router
    {
        private readonly IEnumerable<Route> routes;

        public Router(IRouteProvider provider)
        {
            routes = provider.GetRoutes().ToList();
        }
        
        public RouteMatchResult Match(string path)
        {
            foreach (var route in routes)
            {
                Dictionary<string, string> urlArgs;
                var success = route.TryMatch(path, out urlArgs);

                if (!success)
                    continue;

                return new RouteMatchResult(route, urlArgs);
            }

            return null;
        }
    }
}
