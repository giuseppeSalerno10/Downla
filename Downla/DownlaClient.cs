using Downla.Controller.Interfaces;
using Downla.Interfaces;
using Downla.Models.FileModels;
using Downla.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Downla
{
    public class DownlaClient : IDownlaClient
    {
        public string DownloadPath { get; set; } = $"{Environment.CurrentDirectory}\\DownloadedFiles";
        public int MaxConnections { get; set; } = 10;
        public long MaxPacketSize { get; set; } = 5242880;

        private readonly IFileController _fileController;

        public DownlaClient(IFileController fileController)
        {
            _fileController = fileController;
        }

        /// <summary>
        /// This method will start an asynchronous download operation.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public DownloadInfosModel StartFileDownload(Uri uri, string? authorizationHeader = null, CancellationToken ct = default)
        {
            return _fileController.StartDownload(uri, MaxConnections, DownloadPath, MaxPacketSize, authorizationHeader, ct);
        }

    }
}