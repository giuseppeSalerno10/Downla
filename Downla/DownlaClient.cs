namespace Downla
{
    public class DownlaClient
    {
        private readonly string _basePath = $"{Environment.CurrentDirectory}\\DownloadedFiles";
        private readonly int _maxConnections = 10;
        private readonly long _maxPacketSize = 5242880;

        public DownloadInfosModel DownloadInfos { get; set; } = new DownloadInfosModel() { Status = DownloadStatuses.Downloading };

        // Constructors
        /// <summary>
        /// Downla Constructor
        /// </summary>
        public DownlaClient()
        { }

        /// <summary>
        /// Downla Constructor
        /// </summary>
        /// <param name="directoryPath">Defines the path of the download directory</param>
        public DownlaClient(string directoryPath)
        {
            _basePath = directoryPath;
        }

        /// <summary>
        /// Downla Constructor
        /// </summary>
        /// <param name="maxConnections">Defines the maximum number of concurrent HTTP connections</param>
        /// <param name="directoryPath">Defines the path of the download directory (if null: default value)</param>
        public DownlaClient(int maxConnections, string? directoryPath = null)
        {
            if (maxConnections <= 0) { throw new ArgumentOutOfRangeException(nameof(maxConnections)); }

            _maxConnections = maxConnections;

            if (directoryPath != null)
            {
                _basePath = directoryPath;
            }
        }

        /// <summary>
        /// Downla Constructor
        /// </summary>
        /// <param name="maxConnections">Defines the maximum number of concurrent HTTP connections</param>
        /// <param name="maxPacketSize">Defines the maximum size of a packet</param>
        /// <param name="directoryPath">Defines the path of the download directory (if null: default value)</param>
        public DownlaClient(int maxConnections, long maxPacketSize, string? directoryPath = null)
        {
            if (maxConnections <= 0) { throw new ArgumentOutOfRangeException(nameof(maxConnections)); }
            if (maxPacketSize <= 0) { throw new ArgumentOutOfRangeException(nameof(maxConnections)); }

            _maxConnections = maxConnections;
            _maxPacketSize = maxPacketSize;

            if (directoryPath != null)
            {
                _basePath = directoryPath;
            }
        }

        // Methods
        /// <summary>
        /// This method will start an asynchronous download operation.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public DownloadInfosModel DownloadAsync(Uri uri, CancellationToken ct)
        {
            var task = Task.Run(() => Download(uri, ct), ct);

            return DownloadInfos;
        }

        /// <summary>
        /// This method will start an asynchronous download operation. (with Authorization)
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="ct"></param>
        /// <param name="authorizationHeader"></param>
        /// <returns></returns>
        public DownloadInfosModel DownloadAsync(Uri uri, string authorizationHeader, CancellationToken ct)
        {
            var task = Task.Run(() => Download(uri, ct, authorizationHeader), ct);

            return DownloadInfos;
        }

        /// <summary>
        /// Await for download completion.
        /// Throw an exception if the operation is faulted.
        /// </summary>
        /// <exception cref="Exception">Generic Exception</exception>
        public void EnsureDownload()
        {
            if (DownloadInfos.Status == DownloadStatuses.Downloading)
            {
                DownloadInfos.DownloadTask.Wait();
            }

            if (DownloadInfos.Status == DownloadStatuses.Faulted)
            {
                throw DownloadInfos.Exception;
            }
        }

        private void Download(Uri uri, CancellationToken ct, string? authorizationHeader = null)
        {
            try
            {
                #region Setup

                var connections = new List<ConnectionInfosModel>();

                var partsAvaible = _maxConnections;

                var fileMetadata = HttpConnectionService.GetMetadata(uri, ct)
                    .Result;

                FilesService.CreateFile(_basePath, fileMetadata.Name);

                DownloadInfos.FileName = fileMetadata.Name;
                DownloadInfos.FileDirectory = _basePath;
                DownloadInfos.FileSize = fileMetadata.Size;

                var neededPart = (fileMetadata.Size % _maxPacketSize == 0) ? (int)(fileMetadata.Size / _maxPacketSize) : (int)(fileMetadata.Size / _maxPacketSize) + 1;

                DownloadInfos.TotalPackets = neededPart;

                bool[] fileMap = new bool[neededPart];

                #endregion Setup

                #region Elaboration

                while (DownloadInfos.CurrentSize == 0 || DownloadInfos.CurrentSize < DownloadInfos.FileSize)
                {
                    while ( connections.Count < _maxConnections && DownloadInfos.ActiveConnections + DownloadInfos.DownloadedPackets < DownloadInfos.TotalPackets )
                    {
                        var index = fileMap
                            .Select((value, index) => new { Value = value, Index = index })
                            .First(x => x.Value == false).Index;

                        var startRange = index * _maxPacketSize;
                        var endRange = startRange + _maxPacketSize > DownloadInfos.FileSize ? DownloadInfos.FileSize : startRange + _maxPacketSize;

                        try
                        {
                            Task<HttpResponseMessage> task;

                            if(authorizationHeader is null)
                            {
                                task = HttpConnectionService.GetFileAsync(uri, startRange, endRange, ct);

                            }
                            else
                            {
                                task = HttpConnectionService.GetFileAsync(uri, authorizationHeader, startRange, endRange, ct);
                            }


                            var connectionInfoToAdd = new ConnectionInfosModel()
                            {
                                Task = task,
                                Index = index,
                            };

                            DownloadInfos.ActiveConnections++;
                            connections.Add(connectionInfoToAdd);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Connection creation failed");
                        }

                    }

                    var completedConnections = connections.Where(con => con.Task.IsCompleted || con.Task.IsFaulted).ToArray();

                    foreach (var connection in completedConnections)
                    {
                        if (connection.Task.IsCompleted)
                        {
                            var bytes = HttpConnectionService.ReadBytes(connection.Task.Result)
                                .Result;

                            FilesService.AppendBytes($"{DownloadInfos.FileDirectory}/{DownloadInfos.FileName}", bytes);
                            DownloadInfos.CurrentSize += bytes.Length;

                            DownloadInfos.DownloadedPackets++;

                            fileMap[connection.Index] = true;
                        }

                        DownloadInfos.ActiveConnections--;
                        connections.Remove(connection);
                    }
                }

                #endregion Elaboration

                DownloadInfos.Status = DownloadStatuses.Completed;
            }
            catch (Exception e)
            {
                DownloadInfos.Exception = e;

                DownloadInfos.Status = DownloadStatuses.Faulted;
            }
        }
    }
}