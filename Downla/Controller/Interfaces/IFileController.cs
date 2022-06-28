using Downla.Models;

namespace Downla.Controller.Interfaces
{
    public interface IFileController
    {
        DownlaDownload StartDownloadAsync(Uri uri, int maxConnections, long maxPacketSize, string? authorizationHeader = null, CancellationToken ct = default);
    }
}