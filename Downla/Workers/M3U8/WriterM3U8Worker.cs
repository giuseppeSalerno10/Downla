using Downla.Models;
using Downla.Models.FileModels;
using Downla.Services.Interfaces;
using Downla.Workers.File.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Workers.File
{
    public class WriterM3U8Worker : IWriterM3U8Worker
    {
        private readonly IWritingService _writingService;
        private readonly ILogger<WriterFileWorker> _logger;
        private readonly IHttpConnectionService _connectionService;

        public WriterM3U8Worker(IWritingService writingService, ILogger<WriterFileWorker> logger, IHttpConnectionService connectionService)
        {
            _writingService = writingService;
            _logger = logger;
            _connectionService = connectionService;
        }

        public async Task StartThread(
            DownloadMonitor context,
            int gcFactor,
            SemaphoreSlim downloadSemaphore,
            CustomSortedList<IndexedItem<byte[]>> completedConnections,
            CancellationTokenSource downlaCts
            )
        {
            int wrotePacket = 0;
            int totalPacket;

            string fileName;
            string folderPath;

            lock (context)
            {
                totalPacket = context.Infos.TotalPackets;
                folderPath = context.Infos.FileDirectory;
                fileName = context.Infos.FileName;
            }

            try
            {
                _writingService.Create(folderPath, fileName);

                while (!downlaCts.IsCancellationRequested && wrotePacket < totalPacket)
                {
                    IndexedItem<byte[]> currentPart;

                    await downloadSemaphore.WaitAsync(downlaCts.Token);

                    lock (completedConnections)
                    {
                        currentPart = completedConnections.ElementAt(0);
                        completedConnections.Remove(0);
                    }

                    var bytes = currentPart.Data;

                    _writingService.AppendBytes(folderPath, fileName, ref bytes);

                    if(currentPart.Index % gcFactor == 0)
                    {
                        GC.Collect();
                    }

                    wrotePacket++;
                }

                lock (context)
                {
                    context.Status = DownloadStatuses.Completed;
                }
                
            }
            catch (Exception e)
            {
                _logger.LogError($"[{DateTime.Now}] Downla Writing Error - Message: {e.Message}");

                downlaCts.Cancel();

                lock (context)
                {
                    context.Exceptions.Add(e);
                    if(context.Status != DownloadStatuses.Faulted) { context.Status = DownloadStatuses.Faulted; }
                }
            }
            finally
            {
                completedConnections.Dispose();

            }
        }
    }
}
