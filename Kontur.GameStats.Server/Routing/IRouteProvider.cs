using System.Collections.Generic;

namespace Kontur.GameStats.Server.Routing
{
    public interface IRouteProvider
    {
        IEnumerable<Route> GetRoutes();
    }
}
