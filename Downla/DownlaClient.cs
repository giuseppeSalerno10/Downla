using Downla.Controller.Interfaces;
using Downla.DTOs;
using Downla.Interfaces;
using Downla.Models;
using Downla.Services;
using Downla.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Downla
{
    public class DownlaClient : IDownlaClient
    {
        #region Attributes
        public event OnDownlaEventDelegate? OnStatusChange;
        public event OnDownlaEventDelegate? OnPacketDownloaded;

        public string DownloadPath { get; set; } = $"{Environment.CurrentDirectory}/Downla_Downloads";
        public int MaxConnections { get; set; } = 10;
        public long MaxPacketSize { get; set; } = 5242880;

        private readonly IFileController _fileController;
        private readonly IM3U8Controller _m3U8Controller;

        public DownlaClient(IFileController fileController, IM3U8Controller m3U8Controller, IWritingService writingService)
        {
            _fileController = fileController;
            _m3U8Controller = m3U8Controller;
        }
        #endregion

        /// <summary>
        /// This method will start an asynchronous download operation.
        /// </summary>
        /// <param name="uri">File uri</param>
        /// <param name="sleepTime">sleep everytime the download pipe is full</param>
        /// <param name="authorizationHeader">Authorization header used in the download</param>
        /// <param name="ct">Cancellation Token used to cancel the download</param>
        /// <returns>Download Task</returns>
        public Task<DownloadMonitor> StartFileDownloadAsync(Uri uri, int sleepTime = 0, Dictionary<string,string>? headers = null, CancellationToken ct = default)
        {
            StartFileDownloadAsyncParams par = new()
            {
                SleepTime = sleepTime,
                Uri=uri,
                MaxConnections = MaxConnections,
                MaxPacketSize = MaxPacketSize,
                Headers = headers,
                DownloadPath = DownloadPath,
                CancellationToken = ct,

                OnStatusChange = OnStatusChange,
                OnPacketDownloaded = OnPacketDownloaded
            };

            return _fileController.StartDownloadAsync(par);
        }

        /// <summary>
        /// This method will start an asynchronous m3u8 download operation.
        /// </summary>
        /// <param name="uri">File uri</param>
        /// <param name="fileName">File's name</param>
        /// <param name="sleepTime">Delay between two segment download</param>
        /// <param name="ct">Cancellation Token used to cancel the download</param>
        /// <returns>Download task</returns>
        public Task<DownloadMonitor> StartM3U8DownloadAsync(Uri uri, string fileName, int sleepTime = 0, CancellationToken ct = default)
        {
            StartM3U8DownloadAsyncParams par = new()
            {
                Uri = uri,
                MaxConnections = MaxConnections,
                MaxPacketSize = MaxPacketSize,
                DownloadPath = DownloadPath,
                FileName = fileName,
                SleepTime = sleepTime,
                CancellationToken = ct,

                OnStatusChange = OnStatusChange,
                OnPacketDownloaded = OnPacketDownloaded
            };

            return _m3U8Controller.StartVideoDownloadAsync(par);
        }

    }
}