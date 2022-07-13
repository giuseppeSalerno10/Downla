using Downla.Models.M3U8Models;

namespace Downla.Services
{
    public interface IM3U8UtilitiesService
    {
        Task<M3U8Video> GetM3U8Video(Uri sourceUri, CancellationToken ct);
    }
}