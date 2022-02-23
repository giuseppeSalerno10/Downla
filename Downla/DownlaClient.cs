namespace Downla
{
    public class DownlaClient : IDownlaClient
    {
        private readonly IHttpConnectionService _connectionService;
        private readonly IMimeMapperService _mapperService;
        private readonly IFilesService _filesService;


        public string DownloadPath { get; set; } = $"{Environment.CurrentDirectory}\\DownloadedFiles";
        public int MaxConnections { get; set; } = 10;
        public long MaxPacketSize { get; set; } = 5242880;

        #region Constructors
        /// <summary>
        /// Downla Constructor
        /// </summary>
        public DownlaClient(IHttpConnectionService connectionService, IMimeMapperService mapperService, IFilesService filesService)
        {
            _connectionService = connectionService;
            _mapperService = mapperService;
            _filesService = filesService;
        }


        #endregion

        #region Methods

        /// <summary>
        /// This method will start an asynchronous download operation.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public DownloadInfosModel StartDownload(Uri uri, CancellationToken ct = default)
        {
            var downloadInfos = new DownloadInfosModel() { Status = DownloadStatuses.Downloading };

            downloadInfos.DownloadTask = Task.Run(() => Download(uri, downloadInfos, ct), ct);

            return downloadInfos;
        }


        /// <summary>
        /// This method will start an asynchronous download operation. (with Authorization)
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="ct"></param>
        /// <param name="authorizationHeader"></param>
        /// <returns></returns>
        public DownloadInfosModel StartDownload(Uri uri, string authorizationHeader, CancellationToken ct = default)
        {
            var downloadInfos = new DownloadInfosModel() { Status = DownloadStatuses.Downloading };

            downloadInfos.DownloadTask = Task.Run(() => Download(uri, downloadInfos, ct, authorizationHeader), ct);

            return downloadInfos;
        }

        private async Task Download(Uri uri, DownloadInfosModel downloadInfos, CancellationToken ct = default, string? authorizationHeader = null)
        {

            var completedConnections = new CustomSortedList<ConnectionInfosModel>();
            var activeConnections = new CustomSortedList<ConnectionInfosModel>();

            try
            {
                #region Setup
                var writeIndex = 0;

                var partsAvaible = MaxConnections;

                var fileMetadata = await _connectionService.GetMetadata(uri, ct);

                _filesService.CreateFile(DownloadPath, fileMetadata.Name);

                downloadInfos.FileName = fileMetadata.Name;
                downloadInfos.FileDirectory = DownloadPath;
                downloadInfos.FileSize = fileMetadata.Size;

                var neededPart = (fileMetadata.Size % MaxPacketSize == 0) ? (int)(fileMetadata.Size / MaxPacketSize) : (int)(fileMetadata.Size / MaxPacketSize) + 1;

                downloadInfos.TotalPackets = neededPart;

                Stack<int> indexStack = new Stack<int>();
                for (int i = downloadInfos.TotalPackets - 1; i >= 0; i--)
                {
                    indexStack.Push(i);
                }

                #endregion Setup

                #region Elaboration

                while (downloadInfos.CurrentSize < downloadInfos.FileSize)
                {
                    ct.ThrowIfCancellationRequested();

                    // New requests creation
                    while (activeConnections.Count < MaxConnections && downloadInfos.ActiveConnections + downloadInfos.DownloadedPackets < downloadInfos.TotalPackets)
                    {
                        var fileIndex = indexStack.Pop();

                        var startRange = fileIndex * MaxPacketSize;
                        var endRange = startRange + MaxPacketSize > downloadInfos.FileSize ? downloadInfos.FileSize : startRange + MaxPacketSize - 1;

                        Task<HttpResponseMessage> task = authorizationHeader == null ?
                            Task.Run(() => _connectionService.GetFileRange(uri, startRange, endRange, ct), ct) :
                            Task.Run(() => _connectionService.GetFileRange(uri, authorizationHeader, startRange, endRange, ct), ct);

                        var connectionInfoToAdd = new ConnectionInfosModel()
                        {
                            Task = task,
                            Index = fileIndex,
                        };

                        downloadInfos.ActiveConnections++;
                        activeConnections.Add(connectionInfoToAdd);

                    }

                    // Get completed connections
                    foreach (var connection in activeConnections.ToArray())
                    {
                        if (connection.Task.IsCompleted)
                        {
                            try
                            {
                                var connectionResult = await connection.Task;
                                connectionResult.EnsureSuccessStatusCode();

                                completedConnections.Insert(connection);
                                downloadInfos.DownloadedPackets++;
                            }
                            catch (Exception)
                            {
                                indexStack.Push(connection.Index);
                            }

                            downloadInfos.ActiveConnections--;
                            activeConnections.Remove(connection);
                        }
                    }

                    // Write on file
                    foreach (var completedConnection in completedConnections.ToArray())
                    {
                        if (completedConnection.Index == writeIndex)
                        {
                            var bytes = await _connectionService.ReadBytes(await completedConnection.Task);

                            _filesService.AppendBytes($"{downloadInfos.FileDirectory}/{downloadInfos.FileName}", bytes);
                            downloadInfos.CurrentSize += bytes.Length;

                            writeIndex++;

                            completedConnections.Remove(completedConnection);
                        }
                    }

                }

                #endregion Elaboration

                downloadInfos.Status = DownloadStatuses.Completed;
            }
            catch (Exception e)
            {
                downloadInfos.Exception = e;
                downloadInfos.Status = ct.IsCancellationRequested ? DownloadStatuses.Canceled : DownloadStatuses.Faulted;
                throw;
            }
            finally
            {
                completedConnections.Dispose();
                activeConnections.Dispose();
                downloadInfos.ActiveConnections = 0;
            }
        }
        #endregion

    }
}