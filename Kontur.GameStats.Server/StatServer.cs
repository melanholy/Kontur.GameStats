using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Kontur.GameStats.Server.Routing;

namespace Kontur.GameStats.Server
{
    internal class StatServer : IDisposable
    {
        private readonly HttpListener listener;
        private readonly Router router;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
        private readonly Logger logger;

        public StatServer(IRouteProvider routeProvider)
        {
            logger = new Logger("log.txt");
            listener = new HttpListener();
            router = new Router(routeProvider);
        }

        public void Start(string prefix)
        {
            lock (listener)
            {
                if (isRunning)
                    return;

                listener.Prefixes.Clear();
                listener.Prefixes.Add(prefix);
                listener.Start();

                listenerThread = new Thread(Listen)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                listenerThread.Start();

                isRunning = true;
            }
        }

        public void Stop()
        {
            lock (listener)
            {
                if (!isRunning)
                    return;

                listener.Stop();

                listenerThread.Abort();
                listenerThread.Join();

                isRunning = false;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Stop();

            listener.Close();
        }

        private void Listen()
        {
            while (true)
            {
                HttpListenerContext context;
                try
                {
                    if (listener.IsListening)
                    {
                        context = listener.GetContext();
                        Task.Run(() => HandleContextAsync(context));
                    }
                    else
                        Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    logger.Log($"[{DateTime.Now}]");
                    logger.Log(error.Message);
                    logger.Log(error.StackTrace);
                }
            }
        }

        private string GetErrorMessage(HttpListenerRequest request, int status)
        {
            return string.Format(
                "{0} [{1}] \"{2} {3}\" {4}",
                request.UserHostAddress, DateTime.Now,
                request.HttpMethod, request.Url.AbsolutePath,
                status);
        }
        
        private async Task HandleContextAsync(HttpListenerContext listenerContext)
        {
            HttpResponse httpResponse;
            try
            {
                var match = router.Match(listenerContext.Request.Url.AbsolutePath);
                if (match != null)
                {
                    var request = HttpRequest.FromHttpListenerRequest(listenerContext.Request);
                    httpResponse = await match.Route.HandleAsync(
                        match.UrlArguments, request);
                }
                else
                    httpResponse = new HttpResponse(HttpStatusCode.NotFound);
            }
            catch (Exception error)
            {
                logger.Log(GetErrorMessage(listenerContext.Request, (int) HttpStatusCode.InternalServerError));
                logger.Log(error.Message);
                logger.Log(error.StackTrace);
                httpResponse = new HttpResponse(HttpStatusCode.InternalServerError);
            }

            var statusCode = (int)httpResponse.StatusCode;
            listenerContext.Response.StatusCode = statusCode;
            using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                if (httpResponse.Content != null)
                    await writer.WriteAsync(httpResponse.Content);
            
            if (statusCode >= 400 && statusCode != (int)HttpStatusCode.InternalServerError)
                logger.Log(GetErrorMessage(listenerContext.Request, statusCode));
        }
    }
}