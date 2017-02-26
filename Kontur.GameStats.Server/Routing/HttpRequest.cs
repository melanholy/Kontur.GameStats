using System.IO;
using System.Net;

namespace Kontur.GameStats.Server.Routing
{
    public class HttpRequest
    {
        public string Method { get; }
        public Stream InputStream { get; }

        public HttpRequest(string method, Stream inputStream)
        {
            Method = method;
            InputStream = inputStream;
        }

        public static HttpRequest FromHttpListenerRequest(HttpListenerRequest request)
        {
            return new HttpRequest(request.HttpMethod, request.InputStream);
        }
    }
}
