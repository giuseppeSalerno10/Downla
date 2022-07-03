using Downla.Controller.Interfaces;
using Downla.Managers.Interfaces;
using Downla.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Controller
{
    public class FileController : IFileController
    {
        private readonly IFileManager _manager;
        public FileController(IFileManager manager)
        {
            _manager = manager;
        }

        public Task StartDownloadAsync(
            Uri uri,
            int maxConnections,
            long maxPacketSize,
            out DownloadMonitor downloadMonitor,
            string? authorizationHeader = null,
            CancellationToken ct = default)
        {
            return _manager.StartDownloadAsync(uri, maxConnections, maxPacketSize, out downloadMonitor, authorizationHeader, ct);
        }
    }
}
