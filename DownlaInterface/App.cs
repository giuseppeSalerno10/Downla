using Downla.Interfaces;
using DownlaInterface.Menus.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownlaInterface
{
    public class App
    {
        private readonly IFileMenuManager _fileMenuManager;
        private readonly IM3U8MenuManager _m3u8MenuManager;
        public App(IFileMenuManager fileMenuManager, IM3U8MenuManager m3u8MenuManager)
        {
            _fileMenuManager = fileMenuManager;
            _m3u8MenuManager = m3u8MenuManager;
        }

        public void Start()
        {
            while (true)
            {
                Console.WriteLine("Choose 0 - File Download, 1 M3U8 Download");
                var isSuccess = int.TryParse(Console.ReadKey().KeyChar.ToString(), out int input);
                if (isSuccess)
                {
                    switch (input)
                    {
                        case 0:
                            _fileMenuManager.OpenMenu();
                            break;
                        case 1:
                            _m3u8MenuManager.OpenMenu();
                            break;
                    }

                }

                Console.WriteLine("\n\nPress a key to continue...");
                Console.ReadKey();
            }
        }
    }
}
