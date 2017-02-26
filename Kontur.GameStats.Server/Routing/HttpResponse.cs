using System.Net;

namespace Kontur.GameStats.Server.Routing
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode;
        public string Content;

        public HttpResponse(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        public HttpResponse(HttpStatusCode statusCode, string content)
        {
            StatusCode = statusCode;
            Content = content;
        }
    }
}
