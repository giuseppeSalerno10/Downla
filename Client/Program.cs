using Downla.Core;

Console.WriteLine("Welcome in DownlaClient");

Console.WriteLine("Insert URL");
var url = Console.ReadLine();

/*
Console.WriteLine("Insert Max Parts (0 = Default)");
var maxParts = Convert.ToInt32(Console.ReadLine());
maxParts = maxParts < 1 ? 10 : maxParts;

Console.WriteLine("Insert Max Part Size, in MB (0 = Default)");
var maxPartSize = Convert.ToInt32(Console.ReadLine());
maxPartSize = maxPartSize < 1 ? 10 : maxPartSize;
*/


// https://releases.ubuntu.com/21.10/ubuntu-21.10-desktop-amd64.iso <- EventHandling, URL CRASH



var downla = new DownlaClient();

var cts = new CancellationTokenSource();

var task = downla.StartDownload(url, cts.Token);

while (downla.DownloadInfo.Status.Equals(DownloadStatuses.Downloading))
{
    Console.Clear();
    Console.WriteLine($"Active Parts: {downla.DownloadInfo.ActiveParts}");
    Console.WriteLine($"Total Parts: {downla.DownloadInfo.TotalParts}");
    Console.WriteLine($"Completed Parts: {downla.DownloadInfo.CompletedParts}");
    Console.WriteLine($"File Size: {downla.DownloadInfo.FileSize}");
    Console.WriteLine($"Current Size: {downla.DownloadInfo.CurrentSize}");
    Thread.Sleep(1000);
}

if (downla.DownloadInfo.Status.Equals(DownloadStatuses.Faulted))
{
    Console.WriteLine("ERROR");
}
