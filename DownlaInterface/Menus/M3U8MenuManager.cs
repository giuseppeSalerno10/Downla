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

            _downlaClient.StartM3U8DownloadAsync(uri, fileName, sleepTime: 10);
        }
    }
}
