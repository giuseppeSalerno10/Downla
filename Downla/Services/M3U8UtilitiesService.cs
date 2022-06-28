using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Downla.Services
{
    public class M3U8UtilitiesService : IM3U8UtilitiesService
    {
        public virtual async Task<string[]> GetVideoRecords(Uri uri, CancellationToken ct = default)
        {
            string videoRawData = await GetHttpRawData(uri, HttpMethod.Get, body: null, ct);
            var data = videoRawData
                .Trim()
                .Replace("\r\n", "\n")
                .Split("\n");

            return data
                .Where(record => !string.IsNullOrEmpty(record))
                .ToArray();
        }
        public virtual async Task<string[]> GetPlaylistRecords(Uri uri, CancellationToken ct = default)
        {
            string videoRawData = await GetHttpRawData(uri, HttpMethod.Get, body: null, ct);
            var data = videoRawData
                .Trim()
                .Replace("\r\n", "\n")
                .Split("\n");

            return data
                .Where(record => !string.IsNullOrEmpty(record))
                .ToArray();
        }
        public virtual Task<byte[]> GetSegmentBytes(Uri uri, CancellationToken ct = default)
        {
            return GetHttpBytes(uri, HttpMethod.Get, body: null, ct);
        }

        public virtual Uri GeneratePlaylistUri(Uri initialUri, string playlist)
        {
            var startRemoveIndex = initialUri.AbsoluteUri.LastIndexOf("/");
            var basePath = initialUri.AbsoluteUri.Remove(startRemoveIndex);
            Uri newUri = new Uri($"{basePath}/{playlist}");

            return newUri;
        }
        public virtual Uri GenerateSegmentUri(Uri initialUri, string segment)
        {
            var startRemoveIndex = initialUri.AbsoluteUri.LastIndexOf("/");
            var basePath = initialUri.AbsoluteUri.Remove(startRemoveIndex);
            Uri newUri = new Uri($"{basePath}/{segment}");

            return newUri;
        }


        protected virtual async Task<string> GetHttpRawData(Uri uri, HttpMethod method, object? body, CancellationToken ct)
        {
            var response = await SendAsync(uri, method, body, ct);
            return await response.Content.ReadAsStringAsync(ct);
        }
        protected virtual async Task<byte[]> GetHttpBytes(Uri uri, HttpMethod method, object? body, CancellationToken ct)
        {
            var response = await SendAsync(uri, method, body, ct);
            return await response.Content.ReadAsByteArrayAsync(ct);
        }
        protected virtual async Task<HttpResponseMessage> SendAsync(Uri uri, HttpMethod method, object? body, CancellationToken ct)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(method, uri);

            if (body != null)
            {
                string serializedBody = JsonSerializer.Serialize(body);
                request.Content = new StringContent(serializedBody);
            }

            HttpResponseMessage response = await client.SendAsync(request,ct);

            response.EnsureSuccessStatusCode();

            return response;
        }
    }
}
