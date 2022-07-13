using Downla.Models.M3U8Models;
using Downla.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Downla.Services
{
    public class M3U8UtilitiesService : IM3U8UtilitiesService
    {
        private readonly IHttpConnectionService _connectionService;
        public M3U8UtilitiesService(IHttpConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<M3U8Video> GetM3U8Video(Uri sourceUri, CancellationToken ct)
        {
            var result = new M3U8Video();
            var m3u8PlainBody = await _connectionService.GetHttpRawData(sourceUri, null, ct);

            if (m3u8PlainBody == null)
            {
                throw new ArgumentNullException(nameof(m3u8PlainBody), $"Video {sourceUri} returned null body");
            }

            result.PlainString = m3u8PlainBody;

            string[] records = GetStringArrayFromBody(m3u8PlainBody);

            string componentToDelete = sourceUri.Segments[^1];
            string baseUri = sourceUri.AbsoluteUri.Replace($"/{componentToDelete}", "");

            ConcurrentBag<M3U8Playlist> rawPlaylists = new ConcurrentBag<M3U8Playlist>();

            for (int i = 0; i < records.Length && !ct.IsCancellationRequested; i++)
            {
                string? record = records[i];

                string[] splittedRecord = record.Split(":");
                switch (splittedRecord[0])
                {
                    case "#EXT-X-VERSION":
                        result.Version = splittedRecord[1];
                        break;

                    case "#EXT-X-STREAM-INF":
                        var additionalInfos = splittedRecord[1].Split(",");
                        string playlistUri = records[i + 1].Replace("./", "");

                        string? rawResolution = additionalInfos
                            .FirstOrDefault(info => info.Contains("RESOLUTION"))?
                            .Split("=")[^1]
                            .Split("x")[^1];

                        string? rawBandwidth = additionalInfos
                            .FirstOrDefault(info => info.Contains("BANDWIDTH"))?
                            .Split("=")[1];

                        int resolution = rawResolution is null ? 0: int.Parse(rawResolution);
                        long bandwidth = rawBandwidth is null ? 0 : int.Parse(rawBandwidth);



                        M3U8Playlist rawPlaylist = new M3U8Playlist()
                        {
                            Uri = new Uri($"{baseUri}/{playlistUri}"),
                            Bandwidth = bandwidth,
                            Resolution = resolution
                        };


                        rawPlaylists.Add(rawPlaylist);
                        break;
                    case "#EXT-X-MEDIA":
                        //streamInfosList.First().Infos = qualcosa;
                        break;
                }
            }

            result.Playlists = await GetPlaylists(rawPlaylists, ct);

            return result;


        }

        private async Task<M3U8Playlist[]> GetPlaylists(ConcurrentBag<M3U8Playlist> rawPlaylists, CancellationToken ct)
        {
            HttpClient httpClient = new HttpClient();

            await Parallel.ForEachAsync(rawPlaylists,
                async (rawPlaylist, ct) =>
                {
                    var playlistBody = await _connectionService.GetHttpRawData(rawPlaylist.Uri, null, ct);
                    if(playlistBody == null)
                    {
                        throw new ArgumentNullException(nameof(playlistBody), $"Playlist {rawPlaylist.Uri} returned null body");
                    }
                    var playlistData = GetStringArrayFromBody(playlistBody);

                    var segmentToRemove = rawPlaylist.Uri.Segments[^1];
                    var baseUri = rawPlaylist.Uri.AbsoluteUri.Replace($"/{segmentToRemove}", "");

                    var playlist = ParseDataIntoM3U8Playlist(playlistBody, baseUri);

                    rawPlaylist.Segments = playlist.Segments;
                    rawPlaylist.Header = playlist.Header;

                    rawPlaylist.PlainString = playlistBody;
                });

            return rawPlaylists.ToArray();
        }
        private M3U8Playlist ParseDataIntoM3U8Playlist(string data, string baseUri)
        {
            var result = new M3U8Playlist();

            string[] records = GetStringArrayFromBody(data);

            List<M3U8PlaylistSegment> playlistSegments = new List<M3U8PlaylistSegment>();

            for (int i = 0; i < records.Length; i++)
            {
                string? record = records[i];

                string[] splittedRecord = record.Split(":");

                switch (splittedRecord[0])
                {
                    case "#EXT-X-VERSION":
                        result.Header.Version = splittedRecord[1];
                        break;

                    case "#EXT-X-ALLOW-CACHE":
                        result.Header.IsCacheAllowed = splittedRecord[1];
                        break;

                    case "#EXT-X-TARGETDURATION":
                        result.Header.TargetDuration = splittedRecord[1];
                        break;

                    case "#EXT-X-MEDIA-SEQUENCE":
                        result.Header.MediaSequence = splittedRecord[1];
                        break;

                    case "#EXT-X-PLAYLIST-TYPE":
                        result.Header.PlaylistType = splittedRecord[1];
                        break;


                    case "#EXTINF":
                        var segmentUri = $"{baseUri}/{records[i+1]}";

                        playlistSegments.Add(new M3U8PlaylistSegment
                        {
                            Uri = new Uri(segmentUri)
                        });

                        i++;
                        break;
                }
            }
            result.Segments = playlistSegments
                .ToArray();

            return result;
        }
        private string[] GetStringArrayFromBody(string body)
        {
            var data = body
                .Trim()
                .Replace("\r\n", "\n")
                .Split("\n");

            return data
                .Where(record => !string.IsNullOrEmpty(record))
                .ToArray();
        }

    }
}
