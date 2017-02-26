using System;
using System.Net;

namespace Kontur.GameStats.Server
{
    internal class HttpException : Exception
    {
        public HttpStatusCode Code { get; }

        public HttpException(HttpStatusCode code)
        {
            Code = code;
        }
    }
}
