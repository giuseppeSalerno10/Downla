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
    public class FileManager : IFileManager
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
        public Task StartDownloadAsync(Uri uri,int maxConnections,long maxPacketSize,out DownloadMonitor downloadMonitor,string? authorizationHeader,CancellationToken ct)
        {
            downloadMonitor = new DownloadMonitor() { Status = DownloadStatuses.Pending };
            return Download(downloadMonitor, uri, maxConnections, maxPacketSize, ct, authorizationHeader);
        }



        private async Task Download(
            DownloadMonitor context,
            Uri uri,
            int maxConnections,
            long maxPacketSize,
            CancellationToken ct,
            string? authorizationHeader)
        {

            try
            {
                Stack<int> indexStack = new Stack<int>();

                var fileMetadata = await _connectionService.GetMetadata(uri, ct);

                _filesService.Create(fileMetadata.Name);

                context.Infos.FileName = fileMetadata.Name;
                context.Infos.FileDirectory = _filesService.GeneratePath(fileMetadata.Name);
                context.Infos.FileSize = fileMetadata.Size;

                var neededPart = (fileMetadata.Size % maxPacketSize == 0) ? (int)(fileMetadata.Size / maxPacketSize) : (int)(fileMetadata.Size / maxPacketSize) + 1;

                context.Infos.TotalPackets = neededPart;

                for (int i = context.Infos.TotalPackets - 1; i >= 0; i--)
                {
                    indexStack.Push(i);
                }

                await ElaborateDownload(context, uri, authorizationHeader, maxPacketSize, maxConnections, indexStack, ct);

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
            }
        }
        private async Task ElaborateDownload(
            DownloadMonitor context, 
            Uri uri, 
            string? authorizationHeader, 
            long maxPacketSize, 
            int maxConnections,
            Stack<int> indexStack,
            CancellationToken ct)
        {
            var completedConnections = new CustomSortedList<ConnectionInfosModel<HttpResponseMessage>>();
            var activeConnections = new CustomSortedList<ConnectionInfosModel<HttpResponseMessage>>();
            int writeIndex = 0;

            while (context.Infos.CurrentSize < context.Infos.FileSize)
            {
                ct.ThrowIfCancellationRequested();

                // New requests creation
                while (activeConnections.Count < maxConnections && context.Infos.ActiveConnections + context.Infos.DownloadedPackets < context.Infos.TotalPackets)
                {
                    var fileIndex = indexStack.Pop();

                    var startRange = fileIndex * maxPacketSize;
                    var endRange = startRange + maxPacketSize > context.Infos.FileSize ? context.Infos.FileSize : startRange + maxPacketSize - 1;

                    Task<HttpResponseMessage> task = authorizationHeader == null ?
                        Task.Run(() => _connectionService.GetFileRange(uri, startRange, endRange, ct), ct) :
                        Task.Run(() => _connectionService.GetFileRange(uri, authorizationHeader, startRange, endRange, ct), ct);

                    var connectionInfoToAdd = new ConnectionInfosModel<HttpResponseMessage>()
                    {
                        Task = task,
                        Index = fileIndex,
                    };

                    context.Infos.ActiveConnections++;
                    activeConnections.Add(connectionInfoToAdd);

                }

                // Get completed connections
                foreach (var connection in activeConnections.ToArray())
                {
                    if (connection.Task.IsCompleted)
                    {
                        try
                        {
                            var connectionResult = await connection.Task;
                            connectionResult.EnsureSuccessStatusCode();

                            completedConnections.Insert(connection);
                            context.Infos.DownloadedPackets++;
                        }
                        catch (Exception e)
                        {
                            indexStack.Push(connection.Index);
                            context.Exceptions.Add(e);
                        }

                        context.Infos.ActiveConnections--;
                        activeConnections.Remove(connection);
                    }
                }

                // Write on file
                foreach (var completedConnection in completedConnections.ToArray())
                {
                    if (completedConnection.Index == writeIndex)
                    {
                        var bytes = await _connectionService.ReadBytes(await completedConnection.Task);

                        _filesService.AppendBytes(context.Infos.FileName, bytes);
                        context.Infos.CurrentSize += bytes.Length;

                        writeIndex++;

                        completedConnections.Remove(completedConnection);
                    }
                }

            }
        }
    }
}
