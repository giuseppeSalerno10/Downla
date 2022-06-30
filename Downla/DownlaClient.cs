using Downla.Controller.Interfaces;
using Downla.Interfaces;
using Downla.Models;
using Downla.Services;
using Downla.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Downla
{
    public class DownlaClient : IDownlaClient
    {
        public int MaxConnections { get; set; } = 10;
        public long MaxPacketSize { get; set; } = 5242880;

        private readonly IFileController _fileController;
        private readonly IM3U8Controller _m3U8Controller;

        public DownlaClient(IFileController fileController, IM3U8Controller m3U8Controller)
        {
            _fileController = fileController;
            _m3U8Controller = m3U8Controller;
        }

        /// <summary>
        /// This method will start an asynchronous download operation.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public DownloadMonitor StartFileDownload(Uri uri, string? authorizationHeader = null, CancellationToken ct = default)
        {
            return _fileController.StartDownloadAsync(uri, MaxConnections, MaxPacketSize, authorizationHeader, ct);
        }

        /// <summary>
        /// This method will start an asynchronous m3u8 download operation.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public DownloadMonitor StartM3U8Download(Uri uri, string fileName, int sleepTime, CancellationToken ct = default)
        {
            return _m3U8Controller.StartDownloadVideoAsync(uri, MaxConnections, fileName, sleepTime, ct);
        }
    }
}