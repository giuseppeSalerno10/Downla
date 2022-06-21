using Downla.Models.FileModels;
using Downla.Models.M3U8Models;
using Downla.Services;
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
        public async Task<DownloadInfosModel> DownloadVideo(
            string url,
            int maxConnections,
            string downloadPath,
            long maxPacketSize,
            string fileName,
            IM3U8ReaderService reader,
            CancellationToken ct)
        {
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }

            if (File.Exists($"{downloadPath}/{fileName}"))
            {
                File.Delete($"{downloadPath}/{fileName}");
            }

            List<Task<byte[]>> downloads = new();

            var video = (await GetVideoMetadata(url, reader, ct))
                .Playlists[0];

            foreach (var segment in video.Segments)
            {
                downloads.Add(DownloadSegment(segment.Url, reader, ct));
            }

            foreach (var download in downloads)
            {
                var bytes = await download;

                using (var stream = new FileStream($"{downloadPath}/{fileName}", FileMode.Append))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            return new DownloadInfosModel();
        }

        public async Task<M3U8VideoModel> GetVideoMetadata(string url, IM3U8ReaderService reader, CancellationToken ct)
        {
            M3U8VideoModel videoModel = new M3U8VideoModel() { Url = url };

            string[] records = await reader.GetVideoRecords(url);

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
                        var urlSplitted = url.Split("/");
                        urlSplitted[^1] = records[i + 1]
                            .Replace("./", "");

                        var infoUri = string.Join("/", urlSplitted);

                        var playlist = GetPlaylistMetadata(infoUri, reader, ct);

                        playlistTasks.Add(playlist);

                        i++;
                        break;
                    case "#EXT-X-MEDIA":
                        //streamInfosList.First().Infos = qualcosa;
                        break;
                }
            }

            Task.WaitAll(playlistTasks.ToArray(), ct);

            videoModel.Playlists = new M3U8Playlist[playlistTasks.Count];
            for (int i = 0; i < playlistTasks.Count && !ct.IsCancellationRequested; i++)
            {
                bool isTaken = playlistTasks.TryTake(out Task<M3U8Playlist>? tempTask);
                if (isTaken)
                {
                    videoModel.Playlists[i] = await tempTask!;
                }
            }
            return videoModel;
        }
        public async Task<byte[]> DownloadSegment(string url, IM3U8ReaderService reader, CancellationToken ct)
        {
            return await reader.GetSegmentBytes(url, ct);
        }


        private async Task<M3U8Playlist> GetPlaylistMetadata(string url, IM3U8ReaderService reader, CancellationToken ct)
        {
            M3U8Playlist playlistModel = new M3U8Playlist() { Url = url };

            string[] records = await reader.GetPlaylistRecords(url, ct);

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
                        var splittedUrl = url.Split("/");
                        splittedUrl[^1] = records[i + 1]
                            .Replace("./", "");

                        var segmentUri = string.Join("/", splittedUrl);

                        playlistSegments.Add(new M3U8PlaylistSegment
                        {
                            Url = segmentUri
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
