using Downla.DTOs;
using Downla.Models;
using Downla.Models.FileModels;
using Downla.Models.M3U8Models;
using Downla.Services;
using Downla.Services.Interfaces;
using Downla.Workers.File.Interfaces;
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
        private readonly IWriterM3U8Worker _writerWorker;
        private readonly IDownloaderM3U8Worker _downloadWorker;
        private readonly ILogger<M3U8Manager> _logger;

        private readonly IWritingService _writingService;
        public M3U8Manager(
            ILogger<M3U8Manager> logger,
            IHttpConnectionService connectionService,
            IWriterM3U8Worker writerWorker,
            IDownloaderM3U8Worker downloadWorker,
            IM3U8UtilitiesService m3u8Utilities
,
            IWritingService writingService)
        {
            _connectionService = connectionService;
            _logger = logger;
            _writerWorker = writerWorker;
            _downloadWorker = downloadWorker;
            _m3u8Utilities = m3u8Utilities;
            _writingService = writingService;
        }

        public async Task<DownloadMonitor> StartVideoDownloadAsync(StartM3U8DownloadAsyncParams downloadParams)
        {
            var downloadMonitor = new DownloadMonitor() { Status = DownloadStatuses.Pending };

            CustomSortedList<IndexedItem<byte[]>> completedConnections = new CustomSortedList<IndexedItem<byte[]>>();
            downloadParams.CancellationToken.Register(() =>
            {
                lock (downloadMonitor)
                {
                    if (downloadMonitor.Status == DownloadStatuses.Pending || downloadMonitor.Status == DownloadStatuses.Downloading)
                    {
                        downloadMonitor.Exceptions.Add(new OperationCanceledException("Operation canceled by the user"));
                        downloadMonitor.Status = DownloadStatuses.Canceled;
                    }
                }
            });

            CancellationTokenSource downlaCTS = CancellationTokenSource.CreateLinkedTokenSource(downloadParams.CancellationToken);

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

                SemaphoreSlim downloadSemaphore = new SemaphoreSlim(0, downloadMonitor.Infos.TotalPackets);

                downloadMonitor.Status = DownloadStatuses.Downloading;

                lock (downloadMonitor)
                {
                    downloadMonitor.WriteTask = _writerWorker.StartThread(
                        downloadMonitor,
                        Math.Min(downloadParams.MaxConnections, downloadMonitor.Infos.TotalPackets),
                        downloadSemaphore,
                        completedConnections,
                        downlaCTS
                        );

                    downloadMonitor.DownloadTask = _downloadWorker.StartThread(
                        downloadMonitor,
                        selectedPlaylist,
                        downloadParams.MaxConnections,
                        downloadParams.SleepTime,
                        completedConnections,
                        downloadSemaphore,
                        downlaCTS
                        );
                }
            }
            catch (Exception e)
            {

                lock (downloadMonitor)
                {
                    downloadMonitor.Infos.ActiveConnections = 0;

                    downloadMonitor.Exceptions.Add(e);
                    downloadMonitor.Status = DownloadStatuses.Faulted;
                }
                downlaCTS.Cancel();

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
