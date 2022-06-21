namespace Downla.Services
{
    public interface IM3U8ReaderService
    {
        Task<string[]> GetPlaylistRecords(string url, CancellationToken ct = default);
        Task<byte[]> GetSegmentBytes(string url, CancellationToken ct = default);
        Task<string[]> GetVideoRecords(string url, CancellationToken ct = default);
    }
}