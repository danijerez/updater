using Ionic.Zip;
using Serilog;
using ShellProgressBar;
using System.Diagnostics;

public static class Steps
{
    public static async Task<bool> DownloadFile(IProgress<float>? progress, ProgressBar main, ProgressBarOptions options, string downloadFileUrl, string fileName)
    {
        using var child = main.Spawn(int.MaxValue, "starting download", options);

        try
        {
            using (var client = new HttpClientDownloadWithProgress(downloadFileUrl, fileName))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {

                    if (progressPercentage == null || totalFileSize == null)
                        return;

                    if (progress == null)
                    {
                        progress = child.AsProgress<float>();
                        child.MaxTicks = (int)totalFileSize;
                        child.Message = $"download '{fileName}' to '{downloadFileUrl}'";
                    }

                    progress.Report((float)progressPercentage / 100);
                };

                await client.StartDownload();
                
            }
            main.Tick();

            return true;
        }
        catch (Exception e)
        {
            child.ForegroundColor = ConsoleColor.Red;
            child.WriteErrorLine(e.Message);
            Log.Error(e.Message, e);
            return false;
        }

    }

    public static bool UnZipFile(IProgress<float>? progress, ProgressBar main, ProgressBarOptions options, string fileName, string filePath)
    {
        using var child = main.Spawn(100, $"unzip '{fileName}' in '{filePath}'", options);

        try
        {

            progress = child.AsProgress<float>();
            progress.Report(100);

            using (ZipFile archive = new ZipFile(filePath + fileName))
            {
                archive.RemoveSelectedEntries("updater.exe");
                archive.ExtractAll(filePath, ExtractExistingFileAction.OverwriteSilently);
            }

            main.Tick();
            return true;
        }
        catch (Exception e)
        {
            if (progress != null)
                progress.Report(0);
            child.ForegroundColor = ConsoleColor.Red;
            child.WriteErrorLine(e.Message);
            Log.Error(e.Message, e);
            return false;
        }


    }

    public static bool OpenExe(IProgress<float>? progress, ProgressBar main, ProgressBarOptions options, string exe, string fileName)
    {
        using var child = main.Spawn(100, $"update finished. opening '{exe}'", options);
        try
        {
            progress = child.AsProgress<float>();
            progress.Report(100);
            File.Delete(fileName);
            Process.Start(exe);

            main.Tick();
            return true;
        }
        catch (Exception e)
        {
            if (progress != null)
                progress.Report(0);
            child.ForegroundColor = ConsoleColor.Red;
            child.WriteErrorLine(e.Message);
            Log.Error(e.Message, e);
            return false;
        }

    }
}
