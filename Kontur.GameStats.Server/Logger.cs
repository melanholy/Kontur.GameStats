using System;
using System.IO;

namespace Kontur.GameStats.Server
{
    internal class Logger
    {
        private readonly object _lock = new object();
        private readonly string filename;

        public Logger(string filename)
        {
            this.filename = filename;
        }

        public void Log(string message)
        {
            lock (_lock)
                using (var writer = File.AppendText(filename))
                    writer.WriteLine(message + Environment.NewLine);
        }
    }
}
