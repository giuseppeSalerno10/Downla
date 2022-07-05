using Downla.Models;

namespace Downla.Controller.Interfaces
{
    public interface IFileController
    {
        Task StartDownloadAsync(Uri uri, int maxConnections, long maxPacketSize, string downloadPath, out DownloadMonitor downloadMonitor ,string? authorizationHeader = null, CancellationToken ct = default);
    }
}