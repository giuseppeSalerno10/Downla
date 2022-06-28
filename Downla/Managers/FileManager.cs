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
        public DownlaDownload StartDownload(
            Uri uri,
            int maxConnections,
            long maxPacketSize,
            string? authorizationHeader,
            CancellationToken ct)
        {
            var downloadInfos = new DownlaDownload() { Status = DownloadStatuses.Downloading };

            downloadInfos.Task = Task.Run(() => Download(uri, downloadInfos, maxConnections, maxPacketSize, ct, authorizationHeader), ct);

            return downloadInfos;
        }

        private async Task Download(
            Uri uri,
            DownlaDownload downloadInfos,
            int maxConnections,
            long maxPacketSize,
            CancellationToken ct,
            string? authorizationHeader)
        {

            var completedConnections = new CustomSortedList<ConnectionInfosModel>();
            var activeConnections = new CustomSortedList<ConnectionInfosModel>();

            try
            {
                #region Setup
                var writeIndex = 0;

                var partsAvaible = maxConnections;

                var fileMetadata = await _connectionService.GetMetadata(uri, ct);

                _filesService.Create(fileMetadata.Name);

                downloadInfos.Infos.FileName = fileMetadata.Name;
                downloadInfos.Infos.FileDirectory = _filesService.GeneratePath(fileMetadata.Name);
                downloadInfos.Infos.FileSize = fileMetadata.Size;

                var neededPart = (fileMetadata.Size % maxPacketSize == 0) ? (int)(fileMetadata.Size / maxPacketSize) : (int)(fileMetadata.Size / maxPacketSize) + 1;

                downloadInfos.Infos.TotalPackets = neededPart;

                Stack<int> indexStack = new Stack<int>();
                for (int i = downloadInfos.Infos.TotalPackets - 1; i >= 0; i--)
                {
                    indexStack.Push(i);
                }

                #endregion Setup

                #region Elaboration

                while (downloadInfos.Infos.CurrentSize < downloadInfos.Infos.FileSize)
                {
                    ct.ThrowIfCancellationRequested();

                    // New requests creation
                    while (activeConnections.Count < maxConnections && downloadInfos.Infos.ActiveConnections + downloadInfos.Infos.DownloadedPackets < downloadInfos.Infos.TotalPackets)
                    {
                        var fileIndex = indexStack.Pop();

                        var startRange = fileIndex * maxPacketSize;
                        var endRange = startRange + maxPacketSize > downloadInfos.Infos.FileSize ? downloadInfos.Infos.FileSize : startRange + maxPacketSize - 1;

                        Task<HttpResponseMessage> task = authorizationHeader == null ?
                            Task.Run(() => _connectionService.GetFileRange(uri, startRange, endRange, ct), ct) :
                            Task.Run(() => _connectionService.GetFileRange(uri, authorizationHeader, startRange, endRange, ct), ct);

                        var connectionInfoToAdd = new ConnectionInfosModel()
                        {
                            Task = task,
                            Index = fileIndex,
                        };

                        downloadInfos.Infos.ActiveConnections++;
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
                                downloadInfos.Infos.DownloadedPackets++;
                            }
                            catch (Exception e)
                            {
                                indexStack.Push(connection.Index);
                                downloadInfos.Exceptions.Add(e);
                                _logger.LogError($"[{DateTime.Now}] Downla Error - Message: {e.Message}");
                            }

                            downloadInfos.Infos.ActiveConnections--;
                            activeConnections.Remove(connection);
                        }
                    }

                    // Write on file
                    foreach (var completedConnection in completedConnections.ToArray())
                    {
                        if (completedConnection.Index == writeIndex)
                        {
                            var bytes = await _connectionService.ReadBytes(await completedConnection.Task);

                            _filesService.AppendBytes(downloadInfos.Infos.FileName, bytes);
                            downloadInfos.Infos.CurrentSize += bytes.Length;

                            writeIndex++;

                            completedConnections.Remove(completedConnection);
                        }
                    }

                }

                #endregion Elaboration

                downloadInfos.Status = DownloadStatuses.Completed;
            }
            catch (Exception e)
            {
                downloadInfos.Exceptions.Add(e);
                downloadInfos.Status = ct.IsCancellationRequested ? DownloadStatuses.Canceled : DownloadStatuses.Faulted;
                _logger.LogError($"[{DateTime.Now}] Downla Error - Message: {e.Message}");
                throw;
            }
            finally
            {
                completedConnections.Dispose();
                activeConnections.Dispose();
                downloadInfos.Infos.ActiveConnections = 0;
            }
        }
    }
}
