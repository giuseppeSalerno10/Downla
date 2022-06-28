using Downla.Models;

namespace Downla.Controller.Interfaces
{
    public interface IFileController
    {
        DownlaDownload StartDownload(Uri uri, int maxConnections, long maxPacketSize, string? authorizationHeader = null, CancellationToken ct = default);
    }
}