using Downla.Controller.Interfaces;
using Downla.DTOs;
using Downla.Managers.Interfaces;
using Downla.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Controller
{
    internal class FileController : IFileController
    {
        private readonly IFileManager _manager;
        public FileController(IFileManager manager)
        {
            _manager = manager;
        }

        public Task<DownloadMonitor> StartDownloadAsync(
            StartFileDownloadAsyncParams downloadParams
            )
        {
            return _manager.StartDownloadAsync(downloadParams);
        }
    }
}
