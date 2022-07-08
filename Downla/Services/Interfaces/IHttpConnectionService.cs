using Downla.Models.FileModels;

namespace Downla.Services.Interfaces
{
    public interface IHttpConnectionService
    {
        Task<HttpResponseMessage> GetFileRange(Uri uri, long offset, long count, CancellationToken ct, string? authorizationHeader = null);
        Task<MetadataModel> GetMetadata(Uri uri, CancellationToken ct);
        Task<byte[]> ReadBytes(HttpResponseMessage message);
    }
}