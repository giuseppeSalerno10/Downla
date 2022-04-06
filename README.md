# Downla
Parallel file downloader

https://www.nuget.org/packages/Downla/


# How to use
1. Instantiate a new DownlaClient OR use Dependency Injection (with services.AddDownlaServices())
2. Create a new DownloadInfosModel with the StartDownload(...) method.
3. use downloadInfoModel.EnsureDownloadCompletation(...) to await download completion

You can check for download properties inside the downloadInfoModel

# Downla UI
https://github.com/giuseppeSalerno10/DownlaUI

*The UI use an old downla version (could be broken)
