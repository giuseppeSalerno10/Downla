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

namespace Downla.Workers.File
{
    public class DownloaderFileWorker : IDownloaderFileWorker
    {
        private readonly ILogger<DownloaderFileWorker> _logger;
        private readonly IHttpConnectionService _connectionService;

        public DownloaderFileWorker(ILogger<DownloaderFileWorker> logger, IHttpConnectionService connectionService)
        {
            _logger = logger;
            _connectionService = connectionService;
        }

        public async Task StartThread(
            DownloadMonitor context,
            Uri uri,
            string? authorizationHeader,
            int maxConnections,
            OnDownlaEventDelegate? onPacketDownload,
            CustomSortedList<IndexedItem<HttpResponseMessage>> completedConnections,
            SemaphoreSlim downloadSemaphore,
            CancellationTokenSource downlaCts
            )
        {
            long packetSize;
            int totalPacket;
            long fileSize;

            int downloadedPacket = 0;
            int connections = 0;

            int errorCount = 0;

            long startRange;
            long endRange;

            int fileIndex;

            Stack<int> indexStack = new Stack<int>();
            CustomSortedList<IndexedItem<Task>> activeConnections = new();


            lock (context)
            {
                packetSize = context.Infos.PacketSize;
                fileSize = context.Infos.FileSize;
                totalPacket = context.Infos.TotalPackets;
            }

            var semaphoreMax = Math.Min(maxConnections, totalPacket);
            SemaphoreSlim packetSemaphore = new SemaphoreSlim(0, semaphoreMax);
            packetSemaphore.Release(semaphoreMax);

            try
            {
                for (int i = totalPacket - 1; i >= 0; i--)
                {
                    indexStack.Push(i);
                }

                while (!downlaCts.IsCancellationRequested && downloadedPacket < totalPacket)
                {
                    await packetSemaphore.WaitAsync(downlaCts.Token);

                    errorCount = context.Exceptions.Count;
                    downloadedPacket = context.Infos.DownloadedPackets;
                    connections = context.Infos.ActiveConnections;

                    if (downloadedPacket + connections < totalPacket)
                    {

                        fileIndex = indexStack.Pop();
                        startRange = fileIndex * packetSize;
                        endRange = startRange + packetSize > fileSize ? fileSize : startRange + packetSize - 1;

                        var task = _connectionService.GetFileRange(uri, startRange, endRange, downlaCts.Token, authorizationHeader)
                                                     .ContinueWith(
                                                        (httpMessageTask, fileIndex) => StartDownloadThread(
                                                            httpMessageTask,
                                                            (int)fileIndex!,
                                                            context,
                                                            downloadSemaphore,
                                                            packetSemaphore,
                                                            activeConnections,
                                                            completedConnections,
                                                            indexStack,
                                                            onPacketDownload
                                                            ),
                                                        fileIndex,
                                                        downlaCts.Token
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

                    if (errorCount > 10)
                    {
                        _logger.LogError($"Downla Downloading Error - Message: too many errors - count {errorCount}");

                        downlaCts.Cancel();
                        lock (context)
                        {
                            context.Status = DownloadStatuses.Faulted;
                        }
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Downla Downloading Error - Message: {e.Message}");

                downlaCts.Cancel();

                lock (context)
                {
                    context.Exceptions.Add(e);
                    if (context.Status != DownloadStatuses.Faulted) { context.Status = DownloadStatuses.Faulted; }
                }
            }
            finally
            {
                activeConnections.Dispose();
                indexStack.Clear();
            }

        }


        private async void StartDownloadThread(
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
            catch(Exception e)
            {
                lock (indexStack)
                {
                    context.Exceptions.Add(e);
                    indexStack.Push(fileIndex);
                }
            }
            finally
            {
                lock (context)
                {
                    context.Infos.ActiveConnections--;
                    activeConnections.Remove(new IndexedItem<Task>()
                    {
                        Index = fileIndex,
                        Data = httpResponseMessage
                    });
                }

                packetSemaphore.Release();
            }
        }
    }
}
