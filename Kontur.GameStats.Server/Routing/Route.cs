using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.Routing
{
    public class Route
    {
        private readonly Regex pathRegex;
        private readonly Regex pathVariableRegex = new Regex("<([^>]+)>");
        private readonly Regex pathOptionalRegex = new Regex("\\[(\\/[^\\]]+)\\]");
        private readonly Regex correctPathRegex =
            new Regex("^\\/$|^(?:(?:\\/\\w+)|(?:\\/<\\w+>)|(?:\\[\\/<\\w+>\\])|(?:\\[\\/\\w+\\]))+$");
        private readonly Func<Dictionary<string, string>, HttpRequest, HttpResponse> handler;
        private readonly string[] methods;

        public Route(string path, string[] methods,
            Func<Dictionary<string, string>, HttpRequest, HttpResponse> handler)
        {
            if (!correctPathRegex.IsMatch(path))
                throw new ArgumentException($"Invalid route path: {path}.");

            this.methods = methods;
            this.handler = handler;

            var regexpString = pathOptionalRegex.Replace(path, "(?:$1)?");
            regexpString = pathVariableRegex.Replace(regexpString, "(?<$1>[^/]+)");
            
            pathRegex = new Regex($"^{regexpString}$");
        }

        public Task<HttpResponse> HandleAsync(Dictionary<string, string> urlArguments, HttpRequest request)
        {
            return Task.Run(() => Handle(urlArguments, request));
        }

        public HttpResponse Handle(Dictionary<string, string> urlArguments, HttpRequest request)
        {
            if (!methods.Contains(request.Method))
                return new HttpResponse(HttpStatusCode.MethodNotAllowed);

            return handler(urlArguments, request);
        }

        public bool TryMatch(string path, 
            out Dictionary<string, string> urlArgs)
        {
            var match = pathRegex.Match(path);
            if (!match.Success)
            {
                urlArgs = null;
                return false;
            }

            urlArgs = new Dictionary<string, string>();
            foreach (var groupName in pathRegex.GetGroupNames())
                if (match.Groups[groupName].Value != "")
                    urlArgs[groupName] = match.Groups[groupName].Value;

            return true;
        }
    }
}
