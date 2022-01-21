namespace Downla
{
    public class DownlaClient
    {
        private DownloadInfosModel? downloadInfos;

        public string DownloadPath { get; set; } = $"{Environment.CurrentDirectory}\\DownloadedFiles";
        public int MaxConnections { get; set; } = 10;
        public long MaxPacketSize { get; set; } = 5242880;

        public DownloadInfosModel DownloadInfos 
        { 
            get => downloadInfos ?? throw new ArgumentNullException("DownloadInfos is null"); 
            set => downloadInfos = value; 
        }

        #region Constructors
        /// <summary>
        /// Downla Constructor
        /// </summary>
        public DownlaClient() { }

        /// <summary>
        /// Downla Constructor
        /// </summary>
        /// <param name="directoryPath">Defines the path of the download directory</param>
        public DownlaClient(string directoryPath)
        {
            DownloadPath = directoryPath;
        }

        /// <summary>
        /// Downla Constructor
        /// </summary>
        /// <param name="maxConnections">Defines the maximum number of concurrent HTTP connections</param>
        /// <param name="directoryPath">Defines the path of the download directory (if null: default value)</param>
        public DownlaClient(int maxConnections, string? directoryPath = null)
        {
            if (maxConnections <= 0) { throw new ArgumentOutOfRangeException(nameof(maxConnections)); }

            MaxConnections = maxConnections;

            if (directoryPath != null)
            {
                DownloadPath = directoryPath;
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

            MaxConnections = maxConnections;
            MaxPacketSize = maxPacketSize;

            if (directoryPath != null)
            {
                DownloadPath = directoryPath;
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// This method will start an asynchronous download operation.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public DownloadInfosModel DownloadAsync(Uri uri, CancellationToken ct)
        {
            DownloadInfos = new DownloadInfosModel() { Status = DownloadStatuses.Downloading };

            DownloadInfos.DownloadTask = Task.Run(() => Download(uri, ct), ct);

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
                var writeIndex = 0;

                var completedConnections = new List<ConnectionInfosModel>();
                var activeConnections = new List<ConnectionInfosModel>();

                var partsAvaible = MaxConnections;

                var fileMetadata = HttpConnectionService.GetMetadata(uri, ct)
                    .Result;

                FilesService.CreateFile(DownloadPath, fileMetadata.Name);

                DownloadInfos.FileName = fileMetadata.Name;
                DownloadInfos.FileDirectory = DownloadPath;
                DownloadInfos.FileSize = fileMetadata.Size;

                var neededPart = (fileMetadata.Size % MaxPacketSize == 0) ? (int)(fileMetadata.Size / MaxPacketSize) : (int)(fileMetadata.Size / MaxPacketSize) + 1;

                DownloadInfos.TotalPackets = neededPart;

                Stack<int> indexStack = new Stack<int>();
                for (int i = 0; i < DownloadInfos.TotalPackets; i++)
                {
                    indexStack.Push(i);
                }

                #endregion Setup

                #region Elaboration

                while (DownloadInfos.CurrentSize == 0 || DownloadInfos.CurrentSize < DownloadInfos.FileSize)
                {
                    // New requests creation
                    while (activeConnections.Count < MaxConnections && DownloadInfos.ActiveConnections + DownloadInfos.DownloadedPackets < DownloadInfos.TotalPackets)
                    {
                        var fileIndex = indexStack.Pop();

                        var startRange = fileIndex * MaxPacketSize;
                        var endRange = startRange + MaxPacketSize > DownloadInfos.FileSize ? DownloadInfos.FileSize : startRange + MaxPacketSize;

                        Task<HttpResponseMessage> task = authorizationHeader == null ? 
                            Task.Run(() => HttpConnectionService.GetFileRange(uri, startRange, endRange, ct), ct) :
                            Task.Run(() => HttpConnectionService.GetFileRange(uri, authorizationHeader, startRange, endRange, ct), ct) ;

                        var connectionInfoToAdd = new ConnectionInfosModel()
                        {
                            Task = task,
                            Index = fileIndex,
                        };

                        DownloadInfos.ActiveConnections++;
                        activeConnections.Add(connectionInfoToAdd);

                    }

                    // Get completed connections
                    foreach (var connection in activeConnections.ToArray())
                    {
                        if (connection.Task.IsCompleted)
                        {
                            try
                            {
                                var connectionResult = connection.Task.Result;
                                connectionResult.EnsureSuccessStatusCode();

                                completedConnections.Add(connection);
                                activeConnections.Remove(connection);
                                DownloadInfos.DownloadedPackets++;

                            }
                            catch (Exception)
                            {
                                indexStack.Push(connection.Index);
                            }
                            DownloadInfos.ActiveConnections--;
                        }
                    }

                    // Write on file
                    foreach (var completedConnection in completedConnections.ToArray())
                    {
                        if(completedConnection.Index == writeIndex)
                        {
                            var bytes = HttpConnectionService.ReadBytes(completedConnection.Task.Result)
                               .Result;

                            FilesService.AppendBytes($"{DownloadInfos.FileDirectory}/{DownloadInfos.FileName}", bytes);
                            DownloadInfos.CurrentSize += bytes.Length;

                            writeIndex++;

                            completedConnections.Remove(completedConnection);
                        }
                    }

                }

                #endregion Elaboration

                DownloadInfos.Status = DownloadStatuses.Completed;
            }
            catch (Exception e)
            {
                DownloadInfos.Exception = e;

                DownloadInfos.Status = ct.IsCancellationRequested ? DownloadStatuses.Canceled : DownloadStatuses.Faulted;
                FilesService.DeleteFile(DownloadInfos.FileDirectory, DownloadInfos.FileName);
            }
        }

        #endregion

    }
}