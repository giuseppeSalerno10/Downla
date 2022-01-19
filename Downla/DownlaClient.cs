namespace Downla
{
    public class DownlaClient
    {
        private readonly string _basePath = $"{Environment.CurrentDirectory}\\DownloadedFiles";
        private readonly int _maxConnections = 10;
        private readonly long _maxPacketSize = 5242880;

        private readonly HttpConnectionService _httpConnectionService = new();
        private readonly FilesService _filesService = new();
        public DownloadInfoes DownloadInfo { get; set; } = new DownloadInfoes() { Status = DownloadStatuses.Downloading };

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
        public DownloadInfoes DownloadAsync(Uri uri, CancellationToken ct)
        {
            var task = Task.Run(() => Download(uri, ct), ct);

            return DownloadInfo;
        }

        /// <summary>
        /// Await for download completion.
        /// Throw an exception if the operation is faulted.
        /// </summary>
        /// <exception cref="Exception">Generic Exception</exception>
        public void EnsureDownload()
        {
            if (DownloadInfo.Status == DownloadStatuses.Downloading)
            {
                DownloadInfo.DownloadTask.Wait();
            }

            if (DownloadInfo.Status == DownloadStatuses.Faulted)
            {
                throw new Exception(DownloadInfo.AdditionalInformations);
            }
        }

        private void Download(Uri uri, CancellationToken ct)
        {
            try
            {
                #region Setup

                var connections = new List<ConnectionInfoes>();

                var partsAvaible = _maxConnections;

                var fileMetadata = HttpConnectionService.GetMetadata(uri, ct)
                    .Result;

                FilesService.CreateFile(_basePath, fileMetadata.Name);

                DownloadInfo.FileName = fileMetadata.Name;
                DownloadInfo.FileDirectory = _basePath;
                DownloadInfo.FileSize = fileMetadata.Size;

                var neededPart = (fileMetadata.Size % _maxPacketSize == 0) ? (int)(fileMetadata.Size / _maxPacketSize) : (int)(fileMetadata.Size / _maxPacketSize) + 1;

                DownloadInfo.TotalPackets = neededPart;

                bool[] fileMap = new bool[neededPart];

                #endregion Setup

                #region Elaboration

                while (DownloadInfo.CurrentSize == 0 || DownloadInfo.CurrentSize < DownloadInfo.FileSize)
                {
                    while (
                        connections.Count < _maxConnections &&
                        DownloadInfo.ActiveConnections + DownloadInfo.DownloadedPackets < DownloadInfo.TotalPackets
                        )
                    {
                        var index = fileMap
                            .Select((value, index) => new { Value = value, Index = index })
                            .First(x => x.Value == false).Index;

                        var startRange = index * _maxPacketSize;
                        var endRange = startRange + _maxPacketSize > DownloadInfo.FileSize ? DownloadInfo.FileSize : startRange + _maxPacketSize;

                        var connectionInfoToAdd = new ConnectionInfoes()
                        {
                            Task = HttpConnectionService.GetFileAsync(uri, startRange, endRange, ct),
                            Index = index,
                        };

                        DownloadInfo.ActiveConnections++;
                        connections.Add(connectionInfoToAdd);
                    }

                    var completedConnections = connections.Where(con => con.Task.IsCompleted || con.Task.IsFaulted).ToArray();

                    foreach (var connection in completedConnections)
                    {
                        if (connection.Task.IsCompleted)
                        {
                            var bytes = HttpConnectionService.ReadBytes(connection.Task.Result)
                                .Result;

                            FilesService.AppendBytes($"{DownloadInfo.FileDirectory}/{DownloadInfo.FileName}", bytes);
                            DownloadInfo.CurrentSize += bytes.Length;

                            DownloadInfo.DownloadedPackets++;

                            fileMap[connection.Index] = true;
                        }

                        DownloadInfo.ActiveConnections--;
                        connections.Remove(connection);
                    }
                }

                #endregion Elaboration

                DownloadInfo.Status = DownloadStatuses.Completed;
            }
            catch (Exception e)
            {
                DownloadInfo.AdditionalInformations = e.Message;

                DownloadInfo.Status = DownloadStatuses.Faulted;
            }
        }
    }
}