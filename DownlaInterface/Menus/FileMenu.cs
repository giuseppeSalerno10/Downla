using Downla.Interfaces;
using Downla.Models;
using DownlaInterface.Menus.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            _downlaClient.MaxPacketSize = 20000000;
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
            CancellationTokenSource cts = new CancellationTokenSource();

            var download = await _downlaClient.StartFileDownloadAsync(uri, 100, new Dictionary<string, string>()
            {
                { "Referer", "https://www.animesaturn.cc/watch?file=8aa651RRCl8pm" }
            },cts.Token);

            download.PropertyChanged += Download_PropertyChanged;
            download.Infos.PropertyChanged += Infos_PropertyChanged;

            await download.EnsureDownloadCompletion();
        }

        private void Infos_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName!.Equals(nameof(DownloadMonitor.Infos.DownloadedPackets)))
            {
                Console.WriteLine("Packet Downloaded");
            }
        }

        private void Download_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName!.Equals(nameof(DownloadMonitor.Status)))
            {
                Console.WriteLine("Status Changed");
            }
        }
    }
}
