// See https://aka.ms/new-console-template for more information
using Downla;

Console.WriteLine("Hello, World!");
DownlaClient dc = new DownlaClient();
dc.DownloadAsync(new Uri("https://github.com/notepad-plus-plus/notepad-plus-plus/releases/download/v8.2/npp.8.2.portable.x64.zip"), new CancellationToken());
dc.EnsureDownload();