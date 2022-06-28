using Downla.Models;
using Downla.Models.M3U8Models;
using Downla.Services;

namespace Downla.Controller.Interfaces
{
    public interface IM3U8Controller
    {
        Task<byte[]> DownloadSegment(Uri url, CancellationToken ct = default);
        DownlaDownload DownloadVideo(Uri url, int maxConnections, long maxPacketSize, string fileName, CancellationToken ct = default);
        Task<DownlaM3U8Video> GetVideoMetadata(Uri url, CancellationToken ct = default);
    }
}