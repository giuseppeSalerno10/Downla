using Downla;

var MBSize = Convert.ToInt64(Math.Pow(2, 20));

Console.WriteLine("Welcome in DownlaClient");

#region Input
Console.WriteLine("Insert URL");
var uri = new Uri(Console.ReadLine());

Console.WriteLine("Insert Max Parts (0 = Default)");
int maxParts = Convert.ToInt32(Console.ReadLine());
maxParts = maxParts < 1 ? 1 : maxParts;

Console.WriteLine("Insert Max Part Size, in MB (0 = Default)");
int maxPartSize = Convert.ToInt32(Console.ReadLine());
maxPartSize = maxPartSize < 1 ? 1 : maxPartSize;
#endregion

var downla = new DownlaClient();

var cts = new CancellationTokenSource();

var downloadInfoes = downla.DownloadAsync(uri, cts.Token);

while (downloadInfoes.Status == DownloadStatuses.Downloading)
{
    Console.Clear();
    Console.WriteLine($"Active Connections: {downla.DownloadInfo.ActiveConnections}");
    Console.WriteLine($"Total Packets: {downla.DownloadInfo.TotalPackets}");
    Console.WriteLine($"Downloaded Packets: {downla.DownloadInfo.DownloadedPackets}");
    Console.WriteLine($"File Size: {downla.DownloadInfo.FileSize}");
    Console.WriteLine($"Current Size: {downla.DownloadInfo.CurrentSize}");
    Thread.Sleep(1000);
}

downla.EnsureDownload();

Console.WriteLine("\nDownload Completed.");