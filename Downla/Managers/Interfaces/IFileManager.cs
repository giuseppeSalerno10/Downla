using Downla.DTOs;
using Downla.Models;

namespace Downla.Managers.Interfaces
{
    public interface IFileManager
    {
        Task<DownloadMonitor> StartDownloadAsync(StartFileDownloadAsyncParams downloadParams);
    }
}