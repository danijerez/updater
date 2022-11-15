using Ionic.Zip;
using Serilog;
using ShellProgressBar;
using System.Diagnostics;

public static class Steps
{
    public static async Task<bool> DownloadFile(IProgress<float>? progress, ProgressBar main, ProgressBarOptions options, string downloadFileUrl, string fileName, string filePath)
    {
        using var child = main.Spawn(int.MaxValue, "starting download", options);

        try
        {
            using (var client = new HttpClientDownloadWithProgress(downloadFileUrl, filePath + fileName))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {

                    if (progressPercentage == null || totalFileSize == null)
                        return;

                    if (progress == null)
                    {
                        progress = child.AsProgress<float>();
                        child.MaxTicks = (int)totalFileSize;
                        child.Message = $"download '{fileName}' in {filePath} to '{downloadFileUrl}'";
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

    public static bool UnZipFile(IProgress<float>? progress, ProgressBar main, ProgressBarOptions options, string fileName, string filePath, IEnumerable<string>? ignore)
    {
        using var child = main.Spawn(0, $"unzip '{fileName}' in '{filePath}'", options);

        try
        {

            progress = child.AsProgress<float>();
            using (ZipFile archive = new ZipFile(filePath + fileName))
            {
                if (ignore != null)
                    foreach (var i in ignore)
                        archive.RemoveSelectedEntries(i);

                archive.ExtractProgress += new EventHandler<ExtractProgressEventArgs>((sender, e) => ExtractProgress(sender, e, progress, child, filePath));
                archive.ExtractAll(filePath, ExtractExistingFileAction.OverwriteSilently);
            }
            child.Message = $"unzip successfully '{fileName}' in '{filePath}'";
            child.Tick();
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

    public static void RemoveFilesOrDirectory(IEnumerable<string>? remove, string filePath)
    {
        DirectoryInfo di = new DirectoryInfo(filePath);

        if (remove != null)
        {
            foreach (FileInfo file in di.GetFiles())
                if (remove.Contains(file.Name))
                    file.Delete();
            foreach (DirectoryInfo dir in di.GetDirectories())
                if (remove.Contains(dir.Name))
                    dir.Delete(true);
        }

    }


    public static bool OpenExe(IProgress<float>? progress, ProgressBar main, ProgressBarOptions options, string exe, string filePath)
    {
        using var child = main.Spawn(100, $"updated successfully. opening '{exe}' to {filePath}", options);
        try
        {
            progress = child.AsProgress<float>();
            progress.Report(100);

            Process.Start(filePath + exe);

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

    public static void ExtractProgress(object? sender, ExtractProgressEventArgs e, IProgress<float>? progress, ChildProgressBar pbar, string filePath)
    {
        if (pbar.MaxTicks < (int)e.TotalBytesToTransfer)
            pbar.MaxTicks = (int)e.TotalBytesToTransfer;

        if (e.TotalBytesToTransfer > 0)
        {
            pbar.Message = $"unzip '{e.CurrentEntry.FileName}' in '{filePath}'";
            var p = (float)e.BytesTransferred / (float)e.TotalBytesToTransfer;
            if (progress != null)
                progress.Report(p);
        }
    }

    public static void Wait()
    {
        Console.WriteLine("press any key to close...");
        Console.ReadLine();
        Environment.Exit(0);
    }

    public static void EndWait(ProgressBar pbar)
    {
        pbar.WriteLine("press any key to close...");
        Console.ReadLine();
        Environment.Exit(0);
    }
}
