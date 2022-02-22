
namespace Downla
{
    public interface IHttpConnectionService
    {
        Task<HttpResponseMessage> GetFileRange(Uri uri, long offset, long count, CancellationToken ct);
        Task<HttpResponseMessage> GetFileRange(Uri uri, string authorizationHeader, long offset, long count, CancellationToken ct);
        Task<MetadataModel> GetMetadata(Uri uri, CancellationToken ct);
        Task<byte[]> ReadBytes(HttpResponseMessage message);
    }
}