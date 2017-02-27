using System;
using Fclp;
using InteractivePreGeneratedViews;
using Kontur.GameStats.Server.Database;
using Kontur.GameStats.Server.Routes;
using Kontur.GameStats.Server.Routing;

namespace Kontur.GameStats.Server
{
    public class EntryPoint
    {
        public static void Main(string[] args)
        {
            var commandLineParser = new FluentCommandLineParser<Options>();

            commandLineParser
                .Setup(options => options.Prefix)
                .As("prefix")
                .SetDefault("http://localhost:8080/")
                .WithDescription("HTTP prefix to listen on");

            commandLineParser
                .SetupHelp("h", "help")
                .WithHeader($"{AppDomain.CurrentDomain.FriendlyName} [--prefix <prefix>]")
                .Callback(text => Console.WriteLine(text));

            if (commandLineParser.Parse(args).HelpCalled)
                return;
            
            using (var ctx = new ServerDatabase())
            {
                InteractiveViews
                    .SetViewCacheFactory(
                        ctx,
                        new FileViewCacheFactory(@"views.xml"));
            }

            RunServer(commandLineParser.Object);
        }

        private static void RunServer(Options options)
        {
            var routeFactory = new StatServerRouteProvider();
            using (var server = new StatServer(routeFactory))
            {
                server.Start(options.Prefix);

                Console.ReadKey(true);
            }
        }

        private class Options
        {
            public string Prefix { get; set; }
        }
    }
}
