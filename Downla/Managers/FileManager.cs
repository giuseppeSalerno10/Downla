using Downla.Managers.Interfaces;
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
        private readonly IFilesService _filesService;
        private readonly ILogger<DownlaClient> _logger;

        public FileManager(IHttpConnectionService connectionService, IFilesService filesService, ILogger<DownlaClient> logger)
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
        public DownloadInfosModel StartDownload(
            Uri uri,
            int maxConnections,
            string downloadPath,
            long maxPacketSize,
            string? authorizationHeader,
            CancellationToken ct)
        {
            var downloadInfos = new DownloadInfosModel() { Status = DownloadStatuses.Downloading };

            downloadInfos.DownloadTask = Task.Run(() => Download(uri, downloadInfos, maxConnections, downloadPath, maxPacketSize, ct, authorizationHeader), ct);

            return downloadInfos;
        }

        private async Task Download(
            Uri uri,
            DownloadInfosModel downloadInfos,
            int maxConnections,
            string downloadPath,
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

                _filesService.CreateFile(downloadPath, fileMetadata.Name);

                downloadInfos.FileName = fileMetadata.Name;
                downloadInfos.FileDirectory = downloadPath;
                downloadInfos.FileSize = fileMetadata.Size;

                var neededPart = (fileMetadata.Size % maxPacketSize == 0) ? (int)(fileMetadata.Size / maxPacketSize) : (int)(fileMetadata.Size / maxPacketSize) + 1;

                downloadInfos.TotalPackets = neededPart;

                Stack<int> indexStack = new Stack<int>();
                for (int i = downloadInfos.TotalPackets - 1; i >= 0; i--)
                {
                    indexStack.Push(i);
                }

                #endregion Setup

                #region Elaboration

                while (downloadInfos.CurrentSize < downloadInfos.FileSize)
                {
                    ct.ThrowIfCancellationRequested();

                    // New requests creation
                    while (activeConnections.Count < maxConnections && downloadInfos.ActiveConnections + downloadInfos.DownloadedPackets < downloadInfos.TotalPackets)
                    {
                        var fileIndex = indexStack.Pop();

                        var startRange = fileIndex * maxPacketSize;
                        var endRange = startRange + maxPacketSize > downloadInfos.FileSize ? downloadInfos.FileSize : startRange + maxPacketSize - 1;

                        Task<HttpResponseMessage> task = authorizationHeader == null ?
                            Task.Run(() => _connectionService.GetFileRange(uri, startRange, endRange, ct), ct) :
                            Task.Run(() => _connectionService.GetFileRange(uri, authorizationHeader, startRange, endRange, ct), ct);

                        var connectionInfoToAdd = new ConnectionInfosModel()
                        {
                            Task = task,
                            Index = fileIndex,
                        };

                        downloadInfos.ActiveConnections++;
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
                                downloadInfos.DownloadedPackets++;
                            }
                            catch (Exception e)
                            {
                                indexStack.Push(connection.Index);
                                downloadInfos.Exceptions.Add(e);
                                _logger.LogError($"[{DateTime.Now}] Downla Error - Message: {e.Message}");
                            }

                            downloadInfos.ActiveConnections--;
                            activeConnections.Remove(connection);
                        }
                    }

                    // Write on file
                    foreach (var completedConnection in completedConnections.ToArray())
                    {
                        if (completedConnection.Index == writeIndex)
                        {
                            var bytes = await _connectionService.ReadBytes(await completedConnection.Task);

                            _filesService.AppendBytes($"{downloadInfos.FileDirectory}/{downloadInfos.FileName}", bytes);
                            downloadInfos.CurrentSize += bytes.Length;

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
                downloadInfos.ActiveConnections = 0;
            }
        }
    }
}
