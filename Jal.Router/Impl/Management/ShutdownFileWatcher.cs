using System;
using System.IO;
using System.Threading;
using Jal.Router.Interface.Management;

namespace Jal.Router.Impl.Management
{
    public class ShutdownFileWatcher : IShutdownWatcher
    {
        private FileSystemWatcher _watcher;

        private readonly string _shutdownfile;

        public ShutdownFileWatcher(string shutdownfile)
        {
            _shutdownfile = shutdownfile;
        }

        public void Start(CancellationTokenSource tokensource)
        {
            Console.WriteLine($"Watcher {_shutdownfile}");

            if (string.IsNullOrWhiteSpace(_shutdownfile))
            {
                return;
            }

            var directoryname = Path.GetDirectoryName(_shutdownfile);

            try
            {
                _watcher = new FileSystemWatcher(directoryname);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Watcher exception {ex}");

                throw;
            }

            FileSystemEventHandler onchange = (o, e) =>
            {
                if (!string.IsNullOrWhiteSpace(_shutdownfile))
                {
                    if (e.FullPath.IndexOf(Path.GetFileName(_shutdownfile), StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        tokensource?.Cancel();
                    }
                }
            };

            _watcher.Created += onchange;
            _watcher.Changed += onchange;
            _watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
            _watcher.IncludeSubdirectories = false;
            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watcher = null;
            }
        }
    }
}