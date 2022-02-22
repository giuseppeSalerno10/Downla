using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Downla
{
    public static class DownlaServicesExtensions
    {
        public static IServiceCollection AddDownlaServices(this IServiceCollection services)
        {
            services.AddTransient<IDownlaClient, DownlaClient>();

            services.AddSingleton<IFilesService, FilesService>();
            services.AddSingleton<IHttpConnectionService, HttpConnectionService>();
            services.AddSingleton<IMimeMapperService, MimeMapperService>();

            return services;
        }
    }

}
