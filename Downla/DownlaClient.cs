namespace Downla
{
    public class DownlaClient : IDisposable, IDownlaClient
    {
        private readonly IHttpConnectionService _connectionService;
        private readonly IMimeMapperService _mapperService;
        private readonly IFilesService _filesService;


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
        public DownloadInfosModel StartDownloadAsync(Uri uri, CancellationToken ct)
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
        public DownloadInfosModel StartDownload(Uri uri, string authorizationHeader, CancellationToken ct)
        {
            DownloadInfos = new DownloadInfosModel() { Status = DownloadStatuses.Downloading };

            DownloadInfos.DownloadTask = Task.Run(() => Download(uri, ct, authorizationHeader), ct);

            return DownloadInfos;
        }


        /// <summary>
        /// Await for download completion.
        /// Throw an exception if the operation is faulted.
        /// </summary>
        /// <exception cref="Exception">Generic Exception</exception>
        public async Task AwaitDownloadCompletation(CancellationToken ct)
        {
            try
            {
                await DownloadInfos.DownloadTask;
            }
            catch (Exception)
            {
                throw;
            }
            Dispose();
        }

        private async Task Download(Uri uri, CancellationToken ct, string? authorizationHeader = null)
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

                DownloadInfos.FileName = fileMetadata.Name;
                DownloadInfos.FileDirectory = DownloadPath;
                DownloadInfos.FileSize = fileMetadata.Size;

                var neededPart = (fileMetadata.Size % MaxPacketSize == 0) ? (int)(fileMetadata.Size / MaxPacketSize) : (int)(fileMetadata.Size / MaxPacketSize) + 1;

                DownloadInfos.TotalPackets = neededPart;

                Stack<int> indexStack = new Stack<int>();
                for (int i = DownloadInfos.TotalPackets - 1; i >= 0; i--)
                {
                    indexStack.Push(i);
                }

                #endregion Setup

                #region Elaboration

                while (DownloadInfos.CurrentSize < DownloadInfos.FileSize)
                {
                    ct.ThrowIfCancellationRequested();

                    // New requests creation
                    while (activeConnections.Count < MaxConnections && DownloadInfos.ActiveConnections + DownloadInfos.DownloadedPackets < DownloadInfos.TotalPackets)
                    {
                        var fileIndex = indexStack.Pop();

                        var startRange = fileIndex * MaxPacketSize;
                        var endRange = startRange + MaxPacketSize > DownloadInfos.FileSize ? DownloadInfos.FileSize : startRange + MaxPacketSize - 1;

                        Task<HttpResponseMessage> task = authorizationHeader == null ?
                            Task.Run(() => _connectionService.GetFileRange(uri, startRange, endRange, ct), ct) :
                            Task.Run(() => _connectionService.GetFileRange(uri, authorizationHeader, startRange, endRange, ct), ct);

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
                                var connectionResult = await connection.Task;
                                connectionResult.EnsureSuccessStatusCode();

                                completedConnections.Insert(connection);
                                DownloadInfos.DownloadedPackets++;
                            }
                            catch (Exception)
                            {
                                indexStack.Push(connection.Index);
                            }

                            DownloadInfos.ActiveConnections--;
                            activeConnections.Remove(connection);
                        }
                    }

                    // Write on file
                    foreach (var completedConnection in completedConnections.ToArray())
                    {
                        if (completedConnection.Index == writeIndex)
                        {
                            var bytes = await _connectionService.ReadBytes(await completedConnection.Task);

                            _filesService.AppendBytes($"{DownloadInfos.FileDirectory}/{DownloadInfos.FileName}", bytes);
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
                throw;
            }
            finally
            {
                completedConnections.Dispose();
                activeConnections.Dispose();
                DownloadInfos.ActiveConnections = 0;
            }
        }

        public void Dispose()
        {
            DownloadInfos.DownloadTask.Dispose();
        }

        #endregion

    }
}