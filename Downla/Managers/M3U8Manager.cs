using Downla.Models;
using Downla.Models.FileModels;
using Downla.Models.M3U8Models;
using Downla.Services;
using Downla.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Managers
{
    public class M3U8Manager : IM3U8Manager
    {
        private readonly IM3U8UtilitiesService _m3U8Reader;
        private readonly IWritingService _writingService;
        private readonly ILogger<M3U8Manager> _logger;

        public M3U8Manager(IM3U8UtilitiesService m3U8Reader, IWritingService writingService, ILogger<M3U8Manager> logger)
        {
            _m3U8Reader = m3U8Reader;
            _writingService = writingService;
            _logger = logger;
        }

        public Task StartDownloadVideoAsync(
            Uri uri,
            int maxConnections,
            string downloadPath,
            string fileName,
            int sleepTime,
            out DownloadMonitor downladMonitor,
            CancellationToken ct
            )
        {
            downladMonitor = new DownloadMonitor() { Status = DownloadStatuses.Pending };
            return Download(downladMonitor, uri, maxConnections, downloadPath, fileName, sleepTime, ct);
        }
        public async Task<M3U8Video> GetVideoMetadataAsync(Uri uri, CancellationToken ct)
        {
            M3U8Video videoModel = new M3U8Video() { Uri = uri };

            string[] records = await _m3U8Reader.GetVideoRecords(uri);

            var playlistTasks = new ConcurrentBag<Task<M3U8Playlist>>();

            for (int i = 0; i < records.Length && !ct.IsCancellationRequested; i++)
            {
                string? record = records[i];

                string[] splittedRecord = record.Split(":");
                switch (splittedRecord[0])
                {
                    case "#EXT-X-VERSION":
                        videoModel.Version = splittedRecord[1];
                        break;

                    case "#EXT-X-STREAM-INF":
                        var infoUri = _m3U8Reader.GeneratePlaylistUri(uri, records[i + 1]);

                        var playlist = GetPlaylistMetadata(infoUri, ct);

                        playlistTasks.Add(playlist);

                        i++;
                        break;
                    case "#EXT-X-MEDIA":
                        //streamInfosList.First().Infos = qualcosa;
                        break;
                }
            }

            foreach (var task in playlistTasks)
            {
                await task;
            }

            videoModel.Playlists = new M3U8Playlist[playlistTasks.Count];
            for (int i = 0; i < videoModel.Playlists.Length && !ct.IsCancellationRequested; i++)
            {
                bool isTaken = playlistTasks.TryTake(out Task<M3U8Playlist>? tempTask);
                if (isTaken)
                {
                    videoModel.Playlists[i] = await tempTask!;
                }
            }
            return videoModel;
        }
        public async Task<byte[]> DownloadSegmentAsync(Uri uri, CancellationToken ct)
        {
            return await _m3U8Reader.GetSegmentBytes(uri, ct);
        }




        private async Task<M3U8Playlist> GetPlaylistMetadata(Uri uri, CancellationToken ct)
        {
            M3U8Playlist playlistModel = new M3U8Playlist() { Uri = uri };

            string[] records = await _m3U8Reader.GetPlaylistRecords(uri, ct);

            List<M3U8PlaylistSegment> playlistSegments = new List<M3U8PlaylistSegment>();

            for (int i = 0; i < records.Length && !ct.IsCancellationRequested; i++)
            {
                string? record = records[i];

                string[] splittedRecord = record.Split(":");

                switch (splittedRecord[0])
                {
                    case "#EXT-X-VERSION":
                        playlistModel.Header.Version = splittedRecord[1];
                        break;

                    case "#EXT-X-ALLOW-CACHE":
                        playlistModel.Header.IsCacheAllowed = splittedRecord[1];
                        break;

                    case "#EXT-X-TARGETDURATION":
                        playlistModel.Header.TargetDuration = splittedRecord[1];
                        break;

                    case "#EXT-X-MEDIA-SEQUENCE":
                        playlistModel.Header.MediaSequence = splittedRecord[1];
                        break;

                    case "#EXT-X-PLAYLIST-TYPE":
                        playlistModel.Header.PlaylistType = splittedRecord[1];
                        break;


                    case "#EXTINF":
                        var segmentUri = _m3U8Reader.GenerateSegmentUri(uri, records[i+1]);
                        playlistSegments.Add(new M3U8PlaylistSegment
                        {
                            Uri = segmentUri
                        });

                        i++;
                        break;
                }
            }

            playlistModel.Segments = playlistSegments.ToArray();

            return playlistModel;
        }
        private async Task Download(
            DownloadMonitor context,
            Uri uri,
            int maxConnections,
            string downloadPath,
            string fileName,
            int sleepTime,
            CancellationToken ct)
        {
            try
            {
                var partsAvaible = maxConnections;

                var videoMetadata = await GetVideoMetadataAsync(uri, ct);
                var video = videoMetadata
                    .Playlists[^1];

                _writingService.Create(downloadPath, fileName);

                context.Infos.FileName = fileName;
                context.Infos.FileDirectory = _writingService.GeneratePath(downloadPath, fileName);

                context.Infos.FileSize = 0;

                context.Infos.TotalPackets = video.Segments.Length;

                Stack<int> indexStack = new Stack<int>();
                for (int i = context.Infos.TotalPackets - 1; i >= 0; i--)
                {
                    indexStack.Push(i);
                }

                await ElaborateDownload(context, video, maxConnections, downloadPath, indexStack, sleepTime ,ct);

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
            M3U8Playlist video,
            int maxConnections,
            string downloadPath,
            Stack<int> indexStack,
            int sleepTime,
            CancellationToken ct
            )
        {
            var completedConnections = new CustomSortedList<ConnectionInfosModel<byte[]>>();
            var activeConnections = new CustomSortedList<ConnectionInfosModel<byte[]>>();
            var writeIndex = 0;

            while (indexStack.Any())
            {
                ct.ThrowIfCancellationRequested();

                // New requests creation
                while (activeConnections.Count < maxConnections && context.Infos.ActiveConnections + context.Infos.DownloadedPackets < context.Infos.TotalPackets)
                {
                    Thread.Sleep(sleepTime);

                    var fileIndex = indexStack.Pop();

                    var connectionInfoToAdd = new ConnectionInfosModel<byte[]>()
                    {
                        Task = DownloadSegmentAsync(video.Segments[fileIndex].Uri, ct),
                        Index = fileIndex,
                    };

                    context.Infos.ActiveConnections++;
                    activeConnections.Add(connectionInfoToAdd);

                }

                // Get completed connections
                foreach (var connection in activeConnections.ToArray())
                {
                    try
                    {
                        var connectionResult = await connection.Task;

                        completedConnections.Insert(connection);
                        context.Infos.DownloadedPackets++;
                    }
                    catch (Exception e)
                    {
                        indexStack.Push(connection.Index);
                        context.Exceptions.Add(e);
                        _logger.LogError($"[{DateTime.Now}] Downla Error - Message: {e.Message}");
                    }

                    context.Infos.ActiveConnections--;
                    activeConnections.Remove(connection);
                }

                // Write on file
                foreach (var completedConnection in completedConnections.ToArray())
                {
                    if (completedConnection.Index == writeIndex)
                    {
                        var bytes = await completedConnection.Task;

                        _writingService.AppendBytes(downloadPath, context.Infos.FileName, bytes);
                        context.Infos.CurrentSize += bytes.Length;

                        writeIndex++;

                        completedConnections.Remove(completedConnection);
                    }
                }

            }
        }


    }
}
