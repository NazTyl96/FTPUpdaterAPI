using System.Collections.Concurrent;
using System.Net;
using FTPUpdaterAPI.Utilities;
using FluentFTP;

namespace FTPUpdaterAPI.Services.FTPHandlingService
{
    internal class FTPHandlingService: IFTPHandlingService
    {
        private string? _ftpAddress;
        private readonly NetworkCredential _credentials;
        private readonly string _tempDirectory;

        public FTPHandlingService()
        {
            _credentials = new NetworkCredential("YourLogin", "YourPassword");
            try
            {
                _tempDirectory = DirectoriesUtility.GetDirectoryPath("UpdateTemp");
            }
            catch (NullReferenceException)
            {
                _tempDirectory = Directory.CreateDirectory(Path.Combine(DirectoriesUtility.GetRootDirectoryPath(), "UpdateTemp"))
                                          .ToString();
            }
        }

        async Task<string> IFTPHandlingService.GetVersionFile(string? ftpAddress)
        {
            _ftpAddress = ftpAddress;
            string streamText = "";

            using (AsyncFtpClient client = new AsyncFtpClient(_ftpAddress, _credentials))
            {
                await client.Connect();

                var ms = new MemoryStream();
                await client.DownloadStream(ms, "config.xml");
                ms.Position = 0;

                using (StreamReader reader = new StreamReader(ms))
                {
                    streamText = reader.ReadToEndAsync().Result;
                }
            }

            return streamText;
        }
        async Task IFTPHandlingService.GetFilesForUpdate()
        {
            FtpListItem[]? rootItemsList = null;
            using (AsyncFtpClient client = new AsyncFtpClient(_ftpAddress, _credentials))
            {
                await client.Connect();
                // receiving a list of directories and files in the root of FTP server
                rootItemsList = await client.GetListing();
            }

            // initializing up to 10 parallel connections
            var clients = new ConcurrentBag<FtpClient>();
            var opts = new ParallelOptions { MaxDegreeOfParallelism = 10 };
            Parallel.ForEach(rootItemsList, opts, item =>
            {
                if (item.Name != "config.xml")
                {
                    // a little but of tracing threads/connections to the console
                    string thread = $"Thread {Thread.CurrentThread.ManagedThreadId}";
                    if (!clients.TryTake(out var client))
                    {
                        Console.WriteLine($"{thread} Opening connection...");
                        client = new FtpClient(_ftpAddress, _credentials);
                        client.Connect();
                        Console.WriteLine($"{thread} Opened connection {client.GetHashCode()}.");
                    }

                    string desc =
                        $"{thread}, Connection {client.GetHashCode()}, " +
                        $"File {item.Name} => {_tempDirectory}";
                    Console.WriteLine($"{desc} - Starting...");

                    // downloading a directory with all of its content
                    client.DownloadDirectory(_tempDirectory, item.Name);

                    // output notification of the end of download
                    Console.WriteLine($"{desc} - Done.");

                    clients.Add(client);
                }
            });

            Console.WriteLine($"Closing {clients.Count} connections");
            foreach (var client in clients)
            {
                Console.WriteLine($"Closing connection {client.GetHashCode()}");
                client.Dispose();
            }
        }
    }

}
