using Downla.DTOs;
using Downla.Models;

namespace Downla.Controller.Interfaces
{
    public interface IFileController
    {
        Task StartDownloadAsync(StartFileDownloadAsyncParams downloadParams, out DownloadMonitor downloadMonitor);
    }
}