using Downla.Models.FileModels;

namespace Downla.Controller.Interfaces
{
    public interface IFileController
    {
        DownloadInfosModel StartDownload(Uri uri, int maxConnections, string downloadPath, long maxPacketSize, string? authorizationHeader = null, CancellationToken ct = default);
    }
}