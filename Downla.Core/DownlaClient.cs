using Downla.Files;
using Downla.Http;

namespace Downla.Core
{
    public class DownlaClient
    {
        private string _basePath = $"{Environment.CurrentDirectory}//DownloadedFiles";
        private int _maxParts;
        private int _maxPartSize;
        private HttpConnectionService _httpConnectionService = new HttpConnectionService();
        private FilesService filesService = new FilesService();

        public DownloadInfoes DownloadInfo { get; set;} = new DownloadInfoes() { Status = DownloadStatuses.Downloading };


        public DownlaClient(int maxParts = 10, int maxPartSize = 5242880, string? path = null)
        {
            _maxParts = maxParts;
            _maxPartSize = maxPartSize;

            if (path != null)
            {
                _basePath = path;
            }
        }


        public DownloadInfoes DownloadAsync(Uri uri, CancellationToken ct)
        {
            Task.Run(() => Download(uri,ct));

            return DownloadInfo;
        }

        /// <summary>
        /// Throw an exception if download is faulted
        /// </summary>
        /// <exception cref="Exception">Generic Exception</exception>
        public void EnsureDownload()
        {
            if(DownloadInfo.Status == DownloadStatuses.Faulted)
            {
                throw new Exception(DownloadInfo.AdditionalInformations);
            }
        }

        private void Download(Uri uri, CancellationToken ct)
        {
            try
            {
                #region Setup

                List<ConnectionInfoes> connections = new List<ConnectionInfoes>();

                var partsAvaible = _maxParts;

                var fileMetadata = _httpConnectionService.GetMetadata(uri, ct)
                    .Result;

                filesService.CreateFile(_basePath, fileMetadata.Name);

                DownloadInfo.FileName = fileMetadata.Name;
                DownloadInfo.FileDirectory = _basePath;
                DownloadInfo.FileSize = fileMetadata.Size;

                var neededPart = (fileMetadata.Size % _maxPartSize == 0) ? (int)(fileMetadata.Size / _maxPartSize) : (int)(fileMetadata.Size / _maxPartSize) + 1;

                DownloadInfo.TotalParts = neededPart;

                bool[] fileMap = new bool[neededPart];

                #endregion

                #region Elaboration

                while (DownloadInfo.CurrentSize == 0 || DownloadInfo.CurrentSize < DownloadInfo.FileSize)
                {
                    while (connections.Count < _maxParts)
                    {
                        var index = fileMap
                            .Select((value, index) => new { Value = value, Index = index })
                            .First(x => x.Value == false).Index;

                        var startRange = index * _maxPartSize;
                        var endRange = startRange + _maxPartSize > DownloadInfo.FileSize ? DownloadInfo.FileSize : startRange + _maxPartSize;

                        var connectionInfoToAdd = new ConnectionInfoes()
                        {
                            Task = _httpConnectionService.GetFileAsync(uri, startRange, endRange, ct),
                            Index = index,
                        };

                        DownloadInfo.ActiveParts++;
                        connections.Add(connectionInfoToAdd);

                    }

                    var completedConnections = connections.Where(con => con.Task.IsCompleted || con.Task.IsFaulted).ToArray();

                    foreach (var connection in completedConnections)
                    {
                        if (connection.Task.IsCompleted)
                        {
                            var bytes = _httpConnectionService.ReadBytes(connection.Task.Result)
                                .Result;

                            filesService.AppendBytes($"{DownloadInfo.FileDirectory}/{DownloadInfo.FileName}", bytes);
                            DownloadInfo.CurrentSize += bytes.Length;

                            DownloadInfo.CompletedParts++;

                            fileMap[connection.Index] = true;
                        }

                        DownloadInfo.ActiveParts--;
                        connections.Remove(connection);
                    }

                }
                #endregion

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