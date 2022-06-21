using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Downla.Services
{
    public class M3U8ReaderService : IM3U8ReaderService
    {
        public async Task<string[]> GetVideoRecords(string url, CancellationToken ct = default)
        {
            string videoRawData = await GetHttpRawData(url, HttpMethod.Get, body: null, ct);
            var data = videoRawData
                .Trim()
                .Replace("\r\n", "\n")
                .Split("\n");

            return data
                .Where(record => !string.IsNullOrEmpty(record))
                .ToArray();
        }
        public async Task<string[]> GetPlaylistRecords(string url, CancellationToken ct = default)
        {
            string videoRawData = await GetHttpRawData(url, HttpMethod.Get, body: null, ct);
            var data = videoRawData
                .Trim()
                .Replace("\r\n", "\n")
                .Split("\n");

            return data
                .Where(record => !string.IsNullOrEmpty(record))
                .ToArray();
        }
        public Task<byte[]> GetSegmentBytes(string url, CancellationToken ct = default)
        {
            return GetHttpBytes(url, HttpMethod.Get, body: null, ct);
        }


        private async Task<string> GetHttpRawData(string url, HttpMethod method, object? body, CancellationToken ct)
        {
            var response = await SendAsync(url, method, body, ct);
            return await response.Content.ReadAsStringAsync(ct);
        }
        private async Task<byte[]> GetHttpBytes(string url, HttpMethod method, object? body, CancellationToken ct)
        {
            var response = await SendAsync(url, method, body, ct);
            return await response.Content.ReadAsByteArrayAsync(ct);
        }
        private async Task<HttpResponseMessage> SendAsync(string url, HttpMethod method, object? body, CancellationToken ct)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(method, url);

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
