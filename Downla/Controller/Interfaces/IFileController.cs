using Downla.DTOs;
using Downla.Models;

namespace Downla.Controller.Interfaces
{
    public interface IFileController
    {
        Task<DownloadMonitor> StartDownloadAsync(StartFileDownloadAsyncParams downloadParams);
    }
}