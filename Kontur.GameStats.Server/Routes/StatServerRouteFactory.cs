using System;
using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Routing;

namespace Kontur.GameStats.Server.Routes
{
    public class StatServerRouteFactory : IRouteFactory
    {
        private static readonly List<Route> Routes = new List<Route>();

        public abstract class RouteProvider
        {
            public void RegisterRoute(string path, string[] methods,
                Func<Dictionary<string, string>, HttpRequest, HttpResponse> handler)
            {
                Routes.Add(new Route(path, methods, handler));
            }
        }

        public IEnumerable<Route> GetRoutes()
        {
            // в конструкторе классы добавляют пути через RegisterRoute, и их экземпляры не нужны
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            GetType()
                .Assembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(RouteProvider)) && !type.IsAbstract)
                .Select(Activator.CreateInstance).ToList();

            return Routes;
        }
    }
}
