using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Downla.Controller;
using Downla.Controller.Interfaces;
using Downla.Interfaces;
using Downla.Managers;
using Downla.Managers.Interfaces;
using Downla.Services;
using Downla.Services.Interfaces;
using Downla.Workers.File;
using Downla.Workers.File.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Downla
{
    public static class DownlaServicesExtensions
    {
        public static IServiceCollection AddDownlaServices(this IServiceCollection services, Action<DownlaServiceOptions>? options = null)
        {
            DownlaServiceOptions opt = new(); 
            if(options != null)
            {
                options.Invoke(opt);
            }

            services.AddSingleton<IDownlaClient, DownlaClient>();

            services.AddSingleton<IFileController, FileController>();
            services.AddSingleton<IM3U8Controller, M3U8Controller>();

            services.AddSingleton<IFileManager, FileManager>();
            services.AddSingleton<IM3U8Manager, M3U8Manager>();

            services.AddSingleton<IHttpConnectionService, HttpConnectionService>();
            services.AddSingleton<IMimeMapperService, MimeMapperService>();

            services.AddSingleton<IDownloaderFileWorker, DownloaderFileWorker>();
            services.AddSingleton<IWriterFileWorker, WriterFileWorker>();

            services.AddSingleton(typeof(IM3U8UtilitiesService), opt.M3U8UtilitiesService);
            services.AddSingleton(typeof(IWritingService), opt.WritingService);

            return services;
        }

        public class DownlaServiceOptions 
        {
            public string WritingServicePath { get; set; } = $"{Environment.CurrentDirectory}\\DownloadedFiles";

            internal Type M3U8UtilitiesService { get; private set; } = typeof(M3U8UtilitiesService);
            internal Type WritingService { get; private set; } = typeof(WritingService);

            public void AddWritingService<TWritingService>() where TWritingService : IWritingService
            {
                WritingService = typeof(TWritingService);
            }
            public void AddM3U8UtilitiesService<TM3U8UtilitiesService>() where TM3U8UtilitiesService : IM3U8UtilitiesService
            {
                M3U8UtilitiesService = typeof(TM3U8UtilitiesService);
            }
        }

    }

}
