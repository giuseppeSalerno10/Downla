﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Http
{
    public class HttpConnectionService
    {
        public async Task<Metadata> GetMetadata(string url, CancellationToken ct)
        {
            Metadata metadata;

            var httpClient = new HttpClient();

            var headRequest = new HttpRequestMessage(HttpMethod.Head, url);

            var headResponse = (await httpClient.SendAsync(headRequest, ct))
                .EnsureSuccessStatusCode();

            var headers = headResponse.Content.Headers;

            if (headers.ContentLength is null){ throw new Exception("Lenght is null"); }
            if (headers.ContentLocation is null) { throw new Exception("Name is null"); }
            if (headers.ContentType is null || headers.ContentType.MediaType is null) { throw new Exception("Type is null"); }


            metadata = new Metadata()
            {
                Name = headers.ContentLocation.Fragment,
                Type = headers.ContentType.MediaType,
                Size = (long) headers.ContentLength
            };

            return metadata;
        }

        public Task<HttpResponseMessage> GetFileAsync(string url, long offset, long count, CancellationToken ct)
        {
            var httpClient = new HttpClient();

            var getRequest = new HttpRequestMessage(HttpMethod.Get, url);

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
