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

            services.AddSingleton(opt.ReaderService);
            services.AddSingleton(services =>
            {
                IWritingService writingService = (IWritingService) Activator.CreateInstance(opt.WritingService.GetType())!;
                writingService.WritePath = opt.WritingServicePath;

                return writingService;
            });

            return services;
        }

        public class DownlaServiceOptions 
        {
            public string WritingServicePath { get; set; } = $"{Environment.CurrentDirectory}\\DownloadedFiles";

            public IM3U8UtilitiesService ReaderService { get; set; } = new M3U8UtilitiesService();
            public IWritingService WritingService { get; set; } = new WritingService();
        }

    }

}
