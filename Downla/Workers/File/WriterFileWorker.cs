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
    public class WriterFileWorker : IWriterFileWorker
    {
        private readonly IWritingService _writingService;
        private readonly ILogger<WriterFileWorker> _logger;
        private readonly IHttpConnectionService _connectionService;

        public WriterFileWorker(IWritingService writingService, ILogger<WriterFileWorker> logger, IHttpConnectionService connectionService)
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

            long currentSize = 0;
            long fileSize;
            long packetSize;

            string fileName;
            string folderPath;

            lock (context)
            {
                folderPath = context.Infos.FileDirectory;
                fileName = context.Infos.FileName;
                fileSize = context.Infos.FileSize;
                packetSize = context.Infos.PacketSize;
            }

            try
            {
                _writingService.Create(folderPath, fileName);

                while (!downlaCts.IsCancellationRequested && currentSize < fileSize)
                {
                    IndexedItem<byte[]> currentPart;

                    await downloadSemaphore.WaitAsync(downlaCts.Token);

                    lock (completedConnections)
                    {
                        currentPart = completedConnections.ElementAt(0);
                        completedConnections.Remove(0);
                    }

                    var bytes = currentPart.Data;

                    _writingService.WriteBytes(folderPath, fileName, currentPart.Index * packetSize, ref bytes);

                    currentSize = context.Infos.CurrentSize += bytes.Length;
                    if(currentPart.Index % gcFactor == 0)
                    {
                        GC.Collect();
                    }
                }

                lock (context)
                {
                    context.Status = DownloadStatuses.Completed;
                }
                
            }
            catch (Exception e)
            {
                if (!downlaCts.IsCancellationRequested)
                {
                    _logger.LogError($"Downla Writing Error - Message: {e.Message}");

                    downlaCts.Cancel();

                    lock (context)
                    {
                        context.Exceptions.Add(e);
                        context.Status = DownloadStatuses.Faulted;
                    }
                }

                _writingService.Delete(folderPath, fileName);
            }
            finally
            {
                completedConnections.Dispose();

            }
        }
    }
}
