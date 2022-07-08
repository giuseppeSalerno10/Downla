using Downla.DTOs;
using Downla.Managers.Interfaces;
using Downla.Models;
using Downla.Models.FileModels;
using Downla.Services.Interfaces;
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
        private readonly IHttpConnectionService _connectionService;
        private readonly IWritingService _filesService;
        private readonly ILogger<DownlaClient> _logger;

        public FileManager(IHttpConnectionService connectionService, IWritingService filesService, ILogger<DownlaClient> logger)
        {
            _connectionService = connectionService;
            _filesService = filesService;
            _logger = logger;
        }

        /// <summary>
        /// This method will start an asynchronous download operation.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="ct"></param>
        /// <param name="authorizationHeader"></param>
        /// <returns></returns>
        public Task StartDownloadAsync(StartFileDownloadAsyncParams downloadParams, out DownloadMonitor downloadMonitor)
        {
            downloadMonitor = new DownloadMonitor() { Status = DownloadStatuses.Pending };
            downloadMonitor.OnStatusChange += downloadParams.OnStatusChange;

            return Download(downloadMonitor, downloadParams.Uri, downloadParams.MaxConnections, downloadParams.MaxPacketSize, downloadParams.DownloadPath, downloadParams.AuthorizationHeader, downloadParams.OnPacketDownloaded, downloadParams.CancellationToken);
        }



        private async Task Download(
            DownloadMonitor context,
            Uri uri,
            int maxConnections,
            long maxPacketSize,
            string downloadPath,
            string? authorizationHeader,
            OnDownlaEventDelegate? onIterationStartDelegate,
            CancellationToken ct
            )
        {
            CustomSortedList<IndexedItem<HttpResponseMessage>> completedConnections = new CustomSortedList<IndexedItem<HttpResponseMessage>>();
            CustomSortedList<IndexedItem<Task>> activeConnections = new CustomSortedList<IndexedItem<Task>>();

            Stack<int> indexStack = new Stack<int>();

            try
            {
                var fileMetadata = await _connectionService.GetMetadata(uri, ct);

                _filesService.Create(downloadPath, fileMetadata.Name, fileMetadata.Size);

                context.Infos.FileName = fileMetadata.Name;
                context.Infos.FileDirectory = _filesService.GeneratePath(downloadPath, fileMetadata.Name);
                context.Infos.FileSize = fileMetadata.Size;

                context.Infos.TotalPackets = (fileMetadata.Size % maxPacketSize == 0) ? (int)(fileMetadata.Size / maxPacketSize) : (int)(fileMetadata.Size / maxPacketSize) + 1;

                SemaphoreSlim downloadSemaphore = new SemaphoreSlim(0, context.Infos.TotalPackets);
                SemaphoreSlim packetSemaphore = new SemaphoreSlim(0, maxConnections);
                packetSemaphore.Release(maxConnections);

                for (int i = context.Infos.TotalPackets - 1; i >= 0; i--)
                {
                    indexStack.Push(i);
                }

                Task writeTask = StartWriteThread(context, downloadPath, maxPacketSize, downloadSemaphore, completedConnections, ct);

                await StartDownloadThreads(
                    context, 
                    uri, 
                    authorizationHeader, 
                    maxPacketSize, 
                    indexStack,
                    onIterationStartDelegate, 
                    downloadSemaphore, 
                    packetSemaphore,
                    activeConnections, 
                    completedConnections, 
                    ct
                    );

                await writeTask;

                context.Status = DownloadStatuses.Completed;
            }
            catch (Exception e)
            {
                context.Exceptions.Add(e);
                context.Status = ct.IsCancellationRequested ? DownloadStatuses.Canceled : DownloadStatuses.Faulted;
                _logger.LogError($"[{DateTime.Now}] Downla Error - Message: {e.Message}");
                throw;
            }
            finally
            {
                context.Infos.ActiveConnections = 0;
                completedConnections.Dispose();
                activeConnections.Dispose();
            }
        }


        private async Task StartWriteThread(DownloadMonitor context, string downloadPath, long packetSize, SemaphoreSlim downloadSemaphore, CustomSortedList<IndexedItem<HttpResponseMessage>> completedConnections, CancellationToken ct) 
        {
            long currentSize = 0;
            long fileSize;

            lock (context)
            {
                fileSize = context.Infos.FileSize;
            }

            while (currentSize < fileSize)
            {
                await downloadSemaphore.WaitAsync(ct);
                IndexedItem<HttpResponseMessage> currentPart;

                lock (completedConnections)
                {
                    currentPart = completedConnections.ElementAt(0);
                }

                var bytes = await _connectionService.ReadBytes(currentPart.Data);
                lock (context)
                {
                    _filesService.WriteBytes(downloadPath, context.Infos.FileName, currentPart.Index * packetSize, bytes);
                    currentSize = context.Infos.CurrentSize += bytes.Length;
                }

                lock (completedConnections)
                {
                    completedConnections.Remove(currentPart);
                }
            }
        }

        private async Task StartDownloadThreads(
            DownloadMonitor context,
            Uri uri,
            string? authorizationHeader,
            long maxPacketSize,
            Stack<int> indexStack,
            OnDownlaEventDelegate? onIterationStartDelegate,
            SemaphoreSlim downloadSemaphore,
            SemaphoreSlim packetSemaphore,
            CustomSortedList<IndexedItem<Task>> activeConnections,
            CustomSortedList<IndexedItem<HttpResponseMessage>> completedConnections,
            CancellationToken ct
            )
        {
            int totalPacket;
            long fileSize;
            int downloadedPacket = 0;
            int connections = 0;

            long startRange;
            long endRange;

            int fileIndex;

            lock (context)
            {
                fileSize = context.Infos.FileSize;
                totalPacket = context.Infos.TotalPackets;
            }

            while (
                downloadedPacket < totalPacket)
            {
                ct.ThrowIfCancellationRequested();

                if (downloadedPacket + connections < totalPacket)
                {
                    fileIndex = indexStack.Pop();
                    startRange = fileIndex * maxPacketSize;
                    endRange = startRange + maxPacketSize > fileSize ? fileSize : startRange + maxPacketSize - 1;

                    await packetSemaphore.WaitAsync();

                    var task = _connectionService.GetFileRange(uri, startRange, endRange, ct, authorizationHeader)
                                                 .ContinueWith(
                                                    (httpMessageTask, fileIndex) => DownloadThreadDoWork(httpMessageTask, (int)fileIndex!, context, downloadSemaphore, packetSemaphore, activeConnections, completedConnections, indexStack, onIterationStartDelegate),
                                                    fileIndex,
                                                    ct
                                                    );

                    var connectionInfoToAdd = new IndexedItem<Task>()
                    {
                        Data = task,
                        Index = fileIndex,
                    };

                    lock (activeConnections)
                    {
                        activeConnections.Add(connectionInfoToAdd);
                        context.Infos.ActiveConnections++;
                    }

                }

                lock (context)
                {
                    downloadedPacket = context.Infos.DownloadedPackets;
                    connections = context.Infos.ActiveConnections;
                }

                Thread.Sleep(1000);
            }
        }

        private async void DownloadThreadDoWork(
            Task<HttpResponseMessage> httpResponseMessage, 
            int fileIndex,
            DownloadMonitor context,
            SemaphoreSlim downloadSemaphore,
            SemaphoreSlim packetSemaphore,
            CustomSortedList<IndexedItem<Task>> activeConnections, 
            CustomSortedList<IndexedItem<HttpResponseMessage>> completedConnections,
            Stack<int> indexStack,
            OnDownlaEventDelegate? onIterationStartDelegate
            )
        {
            try
            {
                var response = await httpResponseMessage;
                response.EnsureSuccessStatusCode();

                var indexedItem = new IndexedItem<HttpResponseMessage>()
                {
                    Data = response,
                    Index = fileIndex!
                };
                lock (completedConnections)
                {
                    completedConnections.Insert(indexedItem);
                }

                lock (context)
                {
                    context.Infos.DownloadedPackets++;
                    if (onIterationStartDelegate != null) { onIterationStartDelegate.Invoke(context.Status, context.Infos, context.Exceptions); }
                }

                downloadSemaphore.Release();
            }
            catch
            {
                lock (indexStack)
                {
                    indexStack.Push(fileIndex!);
                }
            }
            finally
            {
                lock (context)
                {
                    context.Infos.ActiveConnections--;
                    activeConnections.Remove(new IndexedItem<Task>()
                    {
                        Index = fileIndex!,
                        Data = httpResponseMessage
                    });
                }

                packetSemaphore.Release();
            }
            
        }
    }
}
