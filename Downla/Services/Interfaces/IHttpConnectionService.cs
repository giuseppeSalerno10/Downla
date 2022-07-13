using Downla.Models.FileModels;

namespace Downla.Services.Interfaces
{
    public interface IHttpConnectionService
    {
        Task<HttpResponseMessage> GetFileRangeAsync(Uri uri, long offset, long count, CancellationToken ct, Dictionary<string, string>? headers);
        Task<byte[]> GetHttpBytes(Uri uri, object? body, CancellationToken ct);
        Task<string?> GetHttpRawData(Uri uri, object? body, CancellationToken ct);
        Task<MetadataModel> GetMetadata(Uri uri, CancellationToken ct);
        Task<byte[]> ReadAsBytesAsync(HttpResponseMessage httpResponseMessage);
        Task<string?> ReadAsStringAsync(HttpResponseMessage httpResponseMessage);
    }
}