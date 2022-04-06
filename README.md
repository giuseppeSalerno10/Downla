# Downla
Parallel file downloader

https://www.nuget.org/packages/Downla/


# How to use
1. Instantiate a new DownlaClient OR use Dipendecy Injection (with services.AddDownlaServices())
2. Create a new DownloadInfosModel with the StartDownload(...) method.
3. use downloadInfoModel.EnsureDownloadCompletation(...) to await download completation

You can check for download properties inside the downloadInfoModel
