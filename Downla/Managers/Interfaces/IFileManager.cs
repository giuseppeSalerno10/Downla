using Downla.Models;

namespace Downla.Managers.Interfaces
{
    public interface IFileManager
    {
        DownlaDownload StartDownloadAsync(
            Uri uri, 
            int maxConnections, 
            long maxPacketSize, 
            string? authorizationHeader, 
            CancellationToken ct
            );
    }
}