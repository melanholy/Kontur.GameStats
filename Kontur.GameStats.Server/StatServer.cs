using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Kontur.GameStats.Server.Routes;
using Kontur.GameStats.Server.Routing;
using IRouteFactory = Kontur.GameStats.Server.Routing.IRouteFactory;

namespace Kontur.GameStats.Server
{
    internal class StatServer : IDisposable
    {
        private readonly HttpListener listener;
        private readonly Router router;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;

        public StatServer(IRouteFactory routeFactory)
        {
            listener = new HttpListener();
            router = new Router(routeFactory);
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
                    // TODO: log errors
                }
            }
        }
        
        private async Task HandleContextAsync(HttpListenerContext listenerContext)
        {
            HttpResponse httpResponse;
            try
            {
                var match = router.Match(listenerContext.Request.Url.AbsolutePath);
                if (match != null)
                    httpResponse = await match.Route.HandleAsync(
                        match.UrlArguments, 
                        HttpRequest.FromHttpListenerRequest(listenerContext.Request));
                else
                    httpResponse = new HttpResponse(HttpStatusCode.NotFound);
            }
            catch
            {
                httpResponse = new HttpResponse(HttpStatusCode.InternalServerError);
            }
            
            listenerContext.Response.StatusCode = (int)httpResponse.StatusCode;
            using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                if (httpResponse.Content != null)
                    await writer.WriteAsync(httpResponse.Content);

            if ((int)httpResponse.StatusCode >= 400)
                throw new HttpException(httpResponse.StatusCode);
        }
    }
}