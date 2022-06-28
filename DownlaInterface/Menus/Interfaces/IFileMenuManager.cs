using Downla.Models;

namespace DownlaInterface.Menus.Interfaces
{
    public interface IFileMenuManager
    {
        void OpenMenu();
        void ShowDownloadInfos(DownlaDownload download);
    }
}