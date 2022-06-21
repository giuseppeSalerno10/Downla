using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Downla.Controller;
using Downla.Controller.Interfaces;
using Downla.Interfaces;
using Downla.Managers;
using Downla.Managers.Interfaces;
using Downla.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Downla
{
    public static class DownlaServicesExtensions
    {
        public static IServiceCollection AddDownlaServices(this IServiceCollection services)
        {
            services.AddSingleton<IDownlaClient, DownlaClient>();

            services.AddSingleton<IFileController, FileController>();
            services.AddSingleton<IM3U8Controller, M3U8Controller>();

            services.AddSingleton<IFileManager, FileManager>();
            services.AddSingleton<IM3U8Manager, M3U8Manager>();

            services.AddSingleton<IFilesService, FilesService>();
            services.AddSingleton<IHttpConnectionService, HttpConnectionService>();
            services.AddSingleton<IMimeMapperService, MimeMapperService>();

            return services;
        }
    }

}
