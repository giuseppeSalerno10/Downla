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
            Dictionary<string, string>? headers,
            int maxConnections,
            int sleepTime,
            CustomSortedList<IndexedItem<byte[]>> completedConnections,
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

                        var task = _connectionService.GetFileRangeAsync(uri, startRange, endRange, downlaCts.Token, headers)
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
                                                            downlaCts
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

                    if (errorCount >= 10)
                    {
                        _logger.LogError($"Downla Downloading Error - Message: too many errors - count {errorCount}");

                        lock (context)
                        {
                            context.Status = DownloadStatuses.Faulted;
                        }

                        downlaCts.Cancel();
                    }

                    await Task.Delay(sleepTime);
                }
            }
            catch (Exception e)
            {
                if (!downlaCts.IsCancellationRequested)
                {
                    _logger.LogError($"Downla Downloading Error - Message: {e.Message}");

                    lock (context)
                    {
                        context.Exceptions.Add(e);
                        context.Status = DownloadStatuses.Faulted;
                    }

                    downlaCts.Cancel();

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
            CustomSortedList<IndexedItem<byte[]>> completedConnections,
            Stack<int> indexStack,
            CancellationTokenSource downlaCts
            )
        {
            try
            {
                var response = await httpResponseMessage;
                byte[] bytes = await _connectionService.ReadAsBytesAsync(response);

                if (response.IsSuccessStatusCode && bytes.Any())
                {
                    var indexedItem = new IndexedItem<byte[]>()
                    {
                        Data = bytes,
                        Index = fileIndex!
                    };
                    lock (completedConnections)
                    {
                        completedConnections.Insert(indexedItem);
                    }

                    lock (context)
                    {
                        context.Infos.DownloadedPackets++;
                    }

                    downloadSemaphore.Release();

                }
                else
                {
                    lock (context)
                    {
                        context.Status = DownloadStatuses.Faulted;
                        context.Exceptions.Add(new HttpRequestException("Download can not proceed", null, response.StatusCode));
                    }

                    downlaCts.Cancel();

                }

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
