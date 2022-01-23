// See https://aka.ms/new-console-template for more information
using Downla;

var ui = new Uri("https://releases.ubuntu.com/21.10/ubuntu-21.10-live-server-amd64.iso");
var cts = new CancellationTokenSource();

var client = new DownlaClient();

var downloadInfos = client.DownloadAsync(ui, cts.Token);
client.EnsureDownload();