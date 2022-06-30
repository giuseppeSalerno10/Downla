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
    public class M3U8MenuManager : IM3U8MenuManager
    {
        private readonly IDownlaClient _downlaClient;

        public M3U8MenuManager(IDownlaClient downlaClient)
        {
            _downlaClient = downlaClient;
        }

        public void OpenMenu()
        {
            Console.Clear();
            Console.WriteLine("M3U8 Menu");

            Console.WriteLine("Insert Url");

            var url = Console.ReadLine();
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception("Bad url");
            }
            var uri = new Uri(url);

            Console.WriteLine("Insert Filename");

            var fileName = Console.ReadLine()!;

            var download = _downlaClient.StartM3U8Download(uri, fileName, startConnectionDelay: 50);

            ShowDownloadInfos(download);
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

            download.EnsureDownloadCompletion()
                .Wait();
            var time = DateTime.Now.Subtract(startDate).TotalSeconds;

            Console.WriteLine($"\nFinal Status: {download.Status}");
            Console.WriteLine($"Time: {time}");
            Console.WriteLine($"Speed (average): {download.Infos.FileSize / time} B/s");
        }
    }
}
