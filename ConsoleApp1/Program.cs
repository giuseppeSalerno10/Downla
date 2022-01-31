// See https://aka.ms/new-console-template for more information
using Downla;

Console.WriteLine("Hello, World!");

var client = new DownlaClient();
client.DownloadAsync(new Uri("https://github.com/giuseppeSalerno10/DownlaUI/releases/edit/v1.0"), CancellationToken.None);
try
{
    client.EnsureDownload(CancellationToken.None);

}
catch (Exception)
{

}

var a = 0;