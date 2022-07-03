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
        }

        public void OpenMenu()
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

            var downloadTask = _downlaClient.StartFileDownloadAsync(uri, out DownloadMonitor downloadMonitor);

            ShowDownloadInfos(downloadMonitor);
        }
        public void ShowDownloadInfos(DownloadMonitor download)
        {
            var startDate = DateTime.Now;
            while (download.Status == Downla.DownloadStatuses.Downloading)
            {
                Console.Clear();
                Console.WriteLine("Download Status");

                Console.WriteLine($"Status: {download.Status}");
                Console.WriteLine($"Percentage: {download.Percentage}");

                Console.WriteLine("\nFile Infos");
                Console.WriteLine($"FileName: {download.Infos.FileName}");
                Console.WriteLine($"FileSize(bytes): {download.Infos.FileSize}");
                Console.WriteLine($"TotalPackets: {download.Infos.TotalPackets}");

                Console.WriteLine("\nCurrent Infos");
                Console.WriteLine($"ActiveConnections: {download.Infos.ActiveConnections}");
                Console.WriteLine($"CurrentSize (bytes): {download.Infos.CurrentSize}");
                Console.WriteLine($"DownloadedPackets: {download.Infos.DownloadedPackets}");
                Thread.Sleep(500);
            }
            var time = DateTime.Now.Subtract(startDate).TotalSeconds;

            Console.WriteLine($"\nFinal Status: {download.Status}");
            Console.WriteLine($"Time: {time}");
            Console.WriteLine($"Speed (average): {download.Infos.FileSize / time} B/s");
        }
    }
}
