using Downla.Models;
using Downla.Models.M3U8Models;
using Downla.Services;
using Downla.Services.Interfaces;
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

        public M3U8Manager(IM3U8UtilitiesService m3U8Reader, IWritingService writingService)
        {
            _m3U8Reader = m3U8Reader;
            _writingService = writingService;
        }

        public DownlaDownload DownloadVideo(
            Uri uri,
            int maxConnections,
            long maxPacketSize,
            string fileName,
            CancellationToken ct)
        {
            var task = Task.Run(async () =>
            {
                _writingService.Create(fileName);

                List<Task<byte[]>> downloads = new();

                var video = (await GetVideoMetadata(uri, ct))
                    .Playlists[0];

                foreach (var segment in video.Segments)
                {
                    downloads.Add(DownloadSegment(segment.Uri, ct));
                }

                foreach (var download in downloads)
                {
                    var bytes = await download;

                    _writingService.AppendBytes(fileName, bytes);
                }
            });
            return new DownlaDownload()
            {
                Task = task,
            };  
        }

        public async Task<DownlaM3U8Video> GetVideoMetadata(Uri uri, CancellationToken ct)
        {
            DownlaM3U8Video videoModel = new DownlaM3U8Video() { Uri = uri };

            string[] records = await _m3U8Reader.GetVideoRecords(uri);

            var playlistTasks = new ConcurrentBag<Task<DownlaM3U8Playlist>>();

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

            Task.WaitAll(playlistTasks.ToArray(), ct);

            videoModel.Playlists = new DownlaM3U8Playlist[playlistTasks.Count];
            for (int i = 0; i < playlistTasks.Count && !ct.IsCancellationRequested; i++)
            {
                bool isTaken = playlistTasks.TryTake(out Task<DownlaM3U8Playlist>? tempTask);
                if (isTaken)
                {
                    videoModel.Playlists[i] = await tempTask!;
                }
            }
            return videoModel;
        }
        public async Task<byte[]> DownloadSegment(Uri uri, CancellationToken ct)
        {
            return await _m3U8Reader.GetSegmentBytes(uri, ct);
        }


        private async Task<DownlaM3U8Playlist> GetPlaylistMetadata(Uri uri, CancellationToken ct)
        {
            DownlaM3U8Playlist playlistModel = new DownlaM3U8Playlist() { Uri = uri };

            string[] records = await _m3U8Reader.GetPlaylistRecords(uri, ct);

            List<DownlaM3U8PlaylistSegment> playlistSegments = new List<DownlaM3U8PlaylistSegment>();

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
                        playlistSegments.Add(new DownlaM3U8PlaylistSegment
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
    }
}
