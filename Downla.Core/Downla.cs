using Downla.Files;
using Downla.Http;

namespace Downla.Core
{
    public class Downla
    {
        private string _basePath = Environment.CurrentDirectory;

        private int _maxParts;
        private int _maxPartSize;

        private HttpConnectionService _httpConnectionService = new HttpConnectionService();
        private FilesService filesService = new FilesService();

        public DownloadInfo? DownloadInfo { get; set;}

        public Downla(int maxParts = 10, int maxPartSize = 50 * 2^20, string? path = null)
        {
            _maxParts = maxParts;
            _maxPartSize = maxPartSize;

            if (path != null)
            {
                _basePath = path;
            }
        }

        public async Task StartDownload(string url, CancellationToken ct)
        {
            #region Setup
            List<ConnectionInfo> connections = new List<ConnectionInfo>();

            var partsAvaible = _maxParts;

            var fileMetadata = await _httpConnectionService.GetMetadata(url, ct);

            filesService.CreateFile(_basePath, fileMetadata.Name);

            DownloadInfo = new DownloadInfo()
            {
                FileName = fileMetadata.Name,
                FilePath = _basePath,
                FileSize = fileMetadata.Size,
            };

            var neededPart = (fileMetadata.Size % _maxParts == 0) ? (int)(fileMetadata.Size / _maxParts) : (int)(fileMetadata.Size / _maxParts) + 1;

            DownloadInfo.TotalParts = neededPart;

            bool[] fileMap = new bool[neededPart];
            #endregion

            #region Elaboration

            while (DownloadInfo.CurrentSize < DownloadInfo.FileSize)
            {
                while (connections.Count < _maxParts)
                {
                    var index = fileMap
                        .Select( (value,index) => new { Value = value, Index = index })
                        .First( x => x.Value == false ).Index;

                    var startRange = index * _maxPartSize;
                    var endRange = startRange + _maxPartSize > DownloadInfo.FileSize ? DownloadInfo.FileSize : startRange + _maxPartSize;

                    var connectionInfoToAdd = new ConnectionInfo()
                    {
                        Task = _httpConnectionService.GetFileAsync(url, startRange, endRange, ct),
                        ConnectionIndex = index,
                    };

                    connections.Add(connectionInfoToAdd);
                }

                connections.Any(req =>
                {
                    if (req.Task.IsCompleted)
                    {
                        connections.Remove(req);
                        fileMap[req.ConnectionIndex] = true;

                        return true;
                    }
                    else if(req.Task.IsFaulted)
                    {
                        connections.Remove(req);
                    }
                    return false;
                });
                
            }
            #endregion

            DownloadInfo.IsCompleted = true;
        }
    }

}