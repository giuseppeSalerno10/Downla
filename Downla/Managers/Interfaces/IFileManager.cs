using Downla.Models;

namespace Downla.Managers.Interfaces
{
    public interface IFileManager
    {
        Task StartDownloadAsync(Uri uri, int maxConnections, long maxPacketSize, string downloadPath, out DownloadMonitor downloadMonitor, string? authorizationHeader, CancellationToken ct);
    }
}