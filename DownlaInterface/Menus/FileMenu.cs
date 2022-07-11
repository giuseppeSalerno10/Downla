using Downla.Interfaces;
using Downla.Models;
using DownlaInterface.Menus.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownlaInterface.Menus
{
    public class FileMenuManager : IFileMenuManager
    {
        private readonly IDownlaClient _downlaClient;

        public FileMenuManager(IDownlaClient downlaClient)
        {
            _downlaClient = downlaClient;

            _downlaClient.MaxPacketSize = 50000000;

            _downlaClient.OnPacketDownloaded += _downlaClient_OnPacketDownloaded;
            _downlaClient.OnStatusChange += _downlaClient_OnStatusChange;
        }

        private void _downlaClient_OnPacketDownloaded(Downla.DownloadStatuses status, DownloadMonitorInfos infos, IEnumerable<Exception> exceptions)
        {
            Console.WriteLine($"PacketDownloaded -> {infos.Percentage}");
        }

        private void _downlaClient_OnStatusChange(Downla.DownloadStatuses status, DownloadMonitorInfos infos, IEnumerable<Exception> exceptions)
        {
            Console.WriteLine($"Status Changed -> {status}");
        }

        public async Task OpenMenu()
        {
            Console.Clear();
            Console.WriteLine("File Menu");

            Console.WriteLine("Insert Url");

            var url = Console.ReadLine();
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception("Bad url");
            }
            var uri = new Uri(url);

            var download = await _downlaClient.StartFileDownloadAsync(uri);
            await download.EnsureDownload();
        }
    }
}
