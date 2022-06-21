using Downla.Models.FileModels;

namespace Downla.Managers.Interfaces
{
    public interface IFileManager
    {
        DownloadInfosModel StartDownload(
            Uri uri, 
            int maxConnections, 
            string downloadPath, 
            long maxPacketSize, 
            string? authorizationHeader, 
            CancellationToken ct
            );
    }
}