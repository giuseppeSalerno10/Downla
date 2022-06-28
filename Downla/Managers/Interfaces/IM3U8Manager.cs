using Downla.Models;
using Downla.Models.M3U8Models;
using Downla.Services;

namespace Downla.Managers
{
    public interface IM3U8Manager
    {
        Task<byte[]> DownloadSegment(Uri uri, CancellationToken ct);
        DownlaDownload DownloadVideo(Uri uri, int maxConnections, long maxPacketSize, string fileName, CancellationToken ct);
        Task<DownlaM3U8Video> GetVideoMetadata(Uri uri, CancellationToken ct);
    }
}