using Downla.Models;

namespace Downla.Managers.Interfaces
{
    public interface IFileManager
    {
        DownlaDownload StartDownload(
            Uri uri, 
            int maxConnections, 
            long maxPacketSize, 
            string? authorizationHeader, 
            CancellationToken ct
            );
    }
}