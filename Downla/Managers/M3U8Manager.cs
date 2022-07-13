using Downla.DTOs;
using Downla.Models;
using Downla.Models.FileModels;
using Downla.Models.M3U8Models;
using Downla.Services;
using Downla.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Managers
{
    internal class M3U8Manager : IM3U8Manager
    {
        private readonly IHttpConnectionService _connectionService;
        private readonly IM3U8UtilitiesService _m3u8Utilities;
        private readonly IWritingService _writingService;
        private readonly ILogger<M3U8Manager> _logger;

        public M3U8Manager(IM3U8UtilitiesService m3U8Reader, IWritingService writingService, ILogger<M3U8Manager> logger, IHttpConnectionService connectionService)
        {
            _m3u8Utilities = m3U8Reader;
            _writingService = writingService;
            _logger = logger;
            _connectionService = connectionService;
        }

        public async Task<DownloadMonitor> StartVideoDownloadAsync(StartM3U8DownloadAsyncParams downloadParams)
        {
            var downloadMonitor = new DownloadMonitor() { Status = DownloadStatuses.Pending };
            downloadMonitor.OnStatusChange += downloadParams.OnStatusChange;

            try
            {
                var metadata = await GetVideoMetadataAsync(downloadParams.Uri, downloadParams.CancellationToken);

                downloadMonitor.Infos.FileName = downloadParams.FileName;
                downloadMonitor.Infos.FileDirectory = downloadParams.DownloadPath;

                var selectedPlaylist = metadata.Playlists.Single( 
                    playlist =>
                    metadata.Playlists.All( 
                        otherPlaylist => 
                        otherPlaylist.Resolution == playlist.Resolution ||
                        otherPlaylist.Resolution < playlist.Resolution
                        )
                    );

                downloadMonitor.Infos.TotalPackets = selectedPlaylist.Segments.Length;

                _writingService.Create(downloadParams.DownloadPath, downloadParams.FileName);

                downloadMonitor.Status = DownloadStatuses.Downloading;

                foreach (var segment in selectedPlaylist.Segments)
                {
                    var bytes = await DownloadSegmentAsync(downloadParams.Uri, downloadParams.CancellationToken);
                    _writingService.AppendBytes(downloadParams.DownloadPath, downloadParams.FileName, ref bytes);
                    await Task.Delay(downloadParams.SleepTime);
                }

                downloadMonitor.Status = DownloadStatuses.Completed;
            }
            catch (Exception e)
            {

                downloadMonitor.Infos.ActiveConnections = 0;
                downloadMonitor.Status = downloadParams.CancellationToken.IsCancellationRequested ? DownloadStatuses.Canceled : DownloadStatuses.Faulted;

                downloadMonitor.DownloadTask.Dispose();
                downloadMonitor.WriteTask.Dispose();
                downloadMonitor.Exceptions.Add(e);

                _logger.LogError($"Downla Error - Message: {e.Message}");
            }

            return downloadMonitor;
        }
        public Task<M3U8Video> GetVideoMetadataAsync(Uri uri, CancellationToken ct)
        {
            return _m3u8Utilities.GetM3U8Video(uri,ct);
        }
        public async Task<byte[]> DownloadSegmentAsync(Uri uri, CancellationToken ct)
        {
            return await _connectionService.GetHttpBytes(uri, null, ct);
        }
    }
}
