using Downla.Models;

namespace Downla.Controller.Interfaces
{
    public interface IFileController
    {
        DownloadMonitor StartDownloadAsync(Uri uri, int maxConnections, long maxPacketSize, string? authorizationHeader = null, CancellationToken ct = default);
    }
}