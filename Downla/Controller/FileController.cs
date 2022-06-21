using Downla.Controller.Interfaces;
using Downla.Managers.Interfaces;
using Downla.Models.FileModels;
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

        public DownloadInfosModel StartDownload(
            Uri uri,
            int maxConnections,
            string downloadPath,
            long maxPacketSize,
            string? authorizationHeader = null,
            CancellationToken ct = default)
        {
            return _manager.StartDownload(uri, maxConnections, downloadPath, maxPacketSize, authorizationHeader, ct);
        }
    }
}
