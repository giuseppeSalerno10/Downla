using Downla.DTOs;
using Downla.Managers.Interfaces;
using Downla.Models;
using Downla.Models.FileModels;
using Downla.Services.Interfaces;
using Downla.Workers.File.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Managers
{
    internal class FileManager : IFileManager
    {
        private readonly ILogger<DownlaClient> _logger;
        private readonly IHttpConnectionService _connectionService;

        private readonly IWriterFileWorker _writerWorker;
        private readonly IDownloaderFileWorker _downloaderFileWorker;

        public FileManager(IHttpConnectionService connectionService, ILogger<DownlaClient> logger, IDownloaderFileWorker downloaderFileWorker, IWriterFileWorker writerWorker)
        {
            _logger = logger;
            _connectionService = connectionService;
            _downloaderFileWorker = downloaderFileWorker;
            _writerWorker = writerWorker;
        }

        public async Task<DownloadMonitor> StartDownloadAsync(StartFileDownloadAsyncParams downloadParams)
        {
            var downloadMonitor = new DownloadMonitor() { Status = DownloadStatuses.Pending };
            downloadMonitor.OnStatusChange += downloadParams.OnStatusChange;

            CustomSortedList<IndexedItem<byte[]>> completedConnections = new CustomSortedList<IndexedItem<byte[]>>();
            downloadParams.CancellationToken.Register(() =>
            {
                lock (downloadMonitor) 
                {
                    downloadMonitor.Status = DownloadStatuses.Canceled;
                }
            });

            CancellationTokenSource downlaCTS = CancellationTokenSource.CreateLinkedTokenSource(downloadParams.CancellationToken);

            try
            {
                var fileMetadata = await _connectionService.GetMetadata(downloadParams.Uri, downloadParams.CancellationToken);

                downloadMonitor.Infos.FileName = fileMetadata.Name;
                downloadMonitor.Infos.FileDirectory = downloadParams.DownloadPath;
                downloadMonitor.Infos.PacketSize = downloadParams.MaxPacketSize;
                downloadMonitor.Infos.FileSize = fileMetadata.Size;

                downloadMonitor.Infos.TotalPackets = (fileMetadata.Size % downloadParams.MaxPacketSize == 0) ? 
                    (int)(fileMetadata.Size / downloadParams.MaxPacketSize) : 
                    (int)(fileMetadata.Size / downloadParams.MaxPacketSize) + 1;

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
                    
                    downloadMonitor.DownloadTask = _downloaderFileWorker.StartThread(
                        downloadMonitor, 
                        downloadParams.Uri, 
                        downloadParams.Headers, 
                        downloadParams.MaxConnections, 
                        downloadParams.SleepTime,
                        downloadParams.OnPacketDownloaded, 
                        completedConnections, 
                        downloadSemaphore,
                        downlaCTS
                        );
                }

            }
            catch (Exception e)
            {

                downlaCTS.Cancel();
                lock (downloadMonitor)
                {
                    downloadMonitor.Infos.ActiveConnections = 0;

                    downloadMonitor.Exceptions.Add(e);
                    downloadMonitor.Status = DownloadStatuses.Faulted;
                }

                _logger.LogError($"Downla Error - Message: {e.Message}");
            }

            return downloadMonitor;
        }


    }
}
