using Downla.DTOs;
using Downla.Models;

namespace Downla.Managers.Interfaces
{
    public interface IFileManager
    {
        Task StartDownloadAsync(StartFileDownloadAsyncParams downloadParams, out DownloadMonitor downloadMonitor);
    }
}