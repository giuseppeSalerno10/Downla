using Downla.Models;

namespace DownlaInterface.Menus.Interfaces
{
    public interface IM3U8MenuManager
    {
        void OpenMenu();
        void ShowDownloadInfos(DownloadMonitor download);
    }
}