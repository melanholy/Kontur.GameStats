using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontur.GameStats.Server.Routing
{
    public class StatServerRouteProvider : IRouteProvider
    {
        private static readonly List<Route> Routes = new List<Route>();
        
        public void RegisterRoute(string path, HttpMethod[] methods,
            Func<Dictionary<string, string>, HttpRequest, HttpResponse> handler)
        {
            Routes.Add(new Route(path, methods, handler));
        }

        public IEnumerable<Route> GetRoutes()
        {
            // в конструкторе классы добавляют пути через RegisterRoute, и их экземпляры не нужны
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            GetType()
                .Assembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(StatServerRouteProvider)) && !type.IsAbstract)
                .Select(Activator.CreateInstance).ToList();

            return Routes;
        }
    }
}
