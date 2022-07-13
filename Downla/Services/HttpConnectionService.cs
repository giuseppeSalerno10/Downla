using Downla.Models.FileModels;
using Downla.Services.Interfaces;
using System.Net;

namespace Downla
{
    public class HttpConnectionService : IHttpConnectionService
    {
        private readonly HttpClient _httpClient = new();


        public async Task<string?> GetHttpRawData(Uri uri, object? body, CancellationToken ct)
        {
            var getRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = _httpClient.SendAsync(getRequest, ct);

            return await ReadAsStringAsync(await response);
        }
        public async Task<byte[]> GetHttpBytes(Uri uri, object? body, CancellationToken ct)
        {
            var getRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = _httpClient.SendAsync(getRequest, ct);

            return await ReadAsBytesAsync(await response);
        }

        public async Task<MetadataModel> GetMetadata(Uri uri, Dictionary<string,string>? requestHeaders, CancellationToken ct)
        {
            MetadataModel metadata;

            var httpClient = new HttpClient();
            var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
            if (requestHeaders != null)
            {
                foreach (var header in requestHeaders)
                {
                    headRequest.Headers.Add(header.Key, header.Value);
                }
            }

            var headResponse = await httpClient.SendAsync(headRequest, ct);

            var headers = headResponse.Content.Headers;

            string name;

            if (headers.ContentLength is null) { throw new Exception("File Lenght is null"); }

            if (headers.ContentDisposition != null && headers.ContentDisposition.FileName != null)
            {
                name = headers.ContentDisposition.FileName;
            }
            else
            {
                name = uri.LocalPath.Split("/")[^1];
            }

            metadata = new MetadataModel()
            {
                Name = name.Replace("\"", ""),
                Size = (long)headers.ContentLength
            };
            return metadata;
        }
        public Task<HttpResponseMessage> GetFileRangeAsync(Uri uri, long offset, long count, CancellationToken ct, Dictionary<string, string>? headers = null)
        {
            var getRequest = new HttpRequestMessage(HttpMethod.Get, uri);

            getRequest.Headers.Add("Range", $"bytes={offset}-{count}");
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    getRequest.Headers.Add(header.Key, header.Value);
                }
            }

            return _httpClient.SendAsync(getRequest, ct);
        }

        public async Task<byte[]> ReadAsBytesAsync(HttpResponseMessage httpResponseMessage)
        {
            byte[] result = Array.Empty<byte>();

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                result = await httpResponseMessage
                    .Content
                    .ReadAsByteArrayAsync();
            }

            return result;
        }
        public async Task<string?> ReadAsStringAsync(HttpResponseMessage httpResponseMessage)
        {
            string? result = null;

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                result = await httpResponseMessage
                    .Content
                    .ReadAsStringAsync();
            }

            return result;
        }
    }
}