using System.Net;

namespace Downla
{
    public class HttpConnectionService
    {
        public static async Task<MetadataModel> GetMetadata(Uri uri, CancellationToken ct)
        {
            MetadataModel metadata;

            var httpClient = new HttpClient();

            var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);

            var headResponse = (await httpClient.SendAsync(headRequest, ct))
                .EnsureSuccessStatusCode();

            var headers = headResponse.Content.Headers;

            string name;

            if (headers.ContentLength is null) { throw new Exception("Lenght is null"); }

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

        public static HttpResponseMessage GetFileRange(Uri uri, long offset, long count, CancellationToken ct)
        {
            var httpClient = new HttpClient();

            var getRequest = new HttpRequestMessage(HttpMethod.Get, uri);

            getRequest.Headers.Add("Range", $"bytes={offset}-{count}");

            var response = httpClient.SendAsync(getRequest, ct).Result;

            return response;
        }

        public static HttpResponseMessage GetFileRange(Uri uri, string authorizationHeader, long offset, long count, CancellationToken ct)
        {
            var httpClient = new HttpClient();

            var getRequest = new HttpRequestMessage(HttpMethod.Get, uri);

            getRequest.Headers.Add("Range", $"bytes={offset}-{count}");
            getRequest.Headers.Add("Authorization", authorizationHeader);

            var response = httpClient.SendAsync(getRequest, ct).Result;

            return response;
        }

        public static async Task<byte[]> ReadBytes(HttpResponseMessage message)
        {
            return await message
                .EnsureSuccessStatusCode()
                .Content
                .ReadAsByteArrayAsync();
        }
    }
}