using System.Collections.Generic;

namespace Kontur.GameStats.Server.Routing
{
    public interface IRouteFactory
    {
        IEnumerable<Route> GetRoutes();
    }
}
