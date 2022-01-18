using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Http
{
    public class HttpConnectionService
    {
        public async Task<Metadata> GetMetadata(Uri uri, CancellationToken ct)
        {
            Metadata metadata;

            var httpClient = new HttpClient();

            var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);

            var headResponse = (await httpClient.SendAsync(headRequest, ct))
                .EnsureSuccessStatusCode();

            var headers = headResponse.Content.Headers;

            string name;

            if (headers.ContentLength is null){ throw new Exception("Lenght is null"); }

            if (headers.ContentDisposition != null && headers.ContentDisposition.FileName != null) 
            { 
                name = headers.ContentDisposition.FileName; 
            } 
            else
            { 
                name = uri.LocalPath.Split("/")[^1];
            }

            metadata = new Metadata()
            {
                Name = name.Replace("\"", ""),
                Size = (long) headers.ContentLength
            };

            return metadata;
        }

        public Task<HttpResponseMessage> GetFileAsync(Uri uri, long offset, long count, CancellationToken ct)
        {
            var httpClient = new HttpClient();

            var getRequest = new HttpRequestMessage(HttpMethod.Get, uri);

            getRequest.Headers.Add("Range", $"bytes={offset}-{count}");

            var task = httpClient.SendAsync(getRequest, ct);

            return task;
        }

        public async Task<byte[]> ReadBytes(HttpResponseMessage message)
        {
            return await message
                .EnsureSuccessStatusCode()
                .Content
                .ReadAsByteArrayAsync();
        }
    }
}
