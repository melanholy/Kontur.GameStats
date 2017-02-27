using System;
using System.IO;
using System.Net;

namespace Kontur.GameStats.Server.Routing
{
    public enum HttpMethod
    {
        Put,
        Get,
        Post
    }

    public class HttpRequest
    {
        public HttpMethod Method { get; }
        public Stream InputStream { get; }

        public HttpRequest(HttpMethod method, Stream inputStream)
        {
            Method = method;
            InputStream = inputStream;
        }

        public static HttpRequest FromHttpListenerRequest(HttpListenerRequest request)
        {
            HttpMethod method;
            if (!Enum.TryParse(request.HttpMethod, true, out method))
                throw new ArgumentException($"Invalid HTTP method: {request.HttpMethod}");

            return new HttpRequest(method, request.InputStream);
        }
    }
}
