namespace Downla.Services
{
    public interface IM3U8UtilitiesService
    {
        Uri GeneratePlaylistUri(Uri initialUri, string playlist);
        Uri GenerateSegmentUri(Uri initialUri, string segment);
        Task<string[]> GetPlaylistRecords(Uri uri, CancellationToken ct = default);
        Task<byte[]> GetSegmentBytes(Uri uri, CancellationToken ct = default);
        Task<string[]> GetVideoRecords(Uri uri, CancellationToken ct = default);
    }
}