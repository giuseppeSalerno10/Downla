using Downla;
using Downla.Workers.File;
using Downla.Workers.File.Interfaces;
using DownlaInterface;
using DownlaInterface.Menus;
using DownlaInterface.Menus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHostBuilder builder = Host.CreateDefaultBuilder();
IHost host = builder.ConfigureServices(
    services =>
    {
        services.AddDownlaServices(options =>
        {
            options.AddWritingService<WritingService>();
        });

        services.AddSingleton<IFileMenuManager, FileMenuManager>();
        services.AddSingleton<IM3U8MenuManager, M3U8MenuManager>();

        services.AddSingleton<App>();
    })
    .Build();

await host.Services.GetRequiredService<App>()
    .Start();