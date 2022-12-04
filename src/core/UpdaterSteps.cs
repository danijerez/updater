using Serilog;
using SharpCompress.Archives;
using ShellProgressBar;
using System.Diagnostics;
using updater.utils;

public static class UpdaterSteps
{
    public static async Task<bool> DownloadFile(IProgress<float>? progress, ProgressBar main, ProgressBarOptions options, ArgOptions arguments)
    {
        using var child = main.Spawn(int.MaxValue, "starting download", options);

        try
        {
            if (arguments.DownloadUrl == null)
                return false;

            using (var client = new HttpClientDownloadWithProgress(arguments.DownloadUrl, arguments.FilePath + arguments.FileName))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {

                    if (progressPercentage == null || totalFileSize == null)
                        return;

                    if (progress == null)
                    {
                        progress = child.AsProgress<float>();
                        child.MaxTicks = (int)totalFileSize;
                        child.Message = $"download '{arguments.FileName}' in {arguments.FilePath} to '{arguments.DownloadUrl}'";
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

    public static bool UnpackDownload(IProgress<float>? progress, ProgressBar main, ProgressBarOptions options, ArgOptions arguments)
    {

        using var child = main.Spawn(0, $"unpack '{arguments.FileName}' in '{arguments.FilePath}'", options);

        try
        {

            if (arguments.FileExtension.Equals(PackFormat.msi))
                return UnpackUtils.UnpackMsi(arguments, child, main);
            else
            {
                progress = child.AsProgress<float>();
                using (var archive = ArchiveFactory.Open(arguments.FilePath + arguments.FileName))
                {
                    return UnpackUtils.UnpackGeneric(progress, archive.Entries.Where(entry => !entry.IsDirectory), arguments, child, main);
                }
            }

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


    public static void RemoveFilesOrDirectory(ArgOptions arguments)
    {

        if (arguments.FilePath == null) return;

        DirectoryInfo di = new DirectoryInfo(arguments.FilePath);

        if (arguments.Remove != null)
        {
            foreach (FileInfo file in di.GetFiles())
                if (arguments.Remove.Contains(file.Name))
                    file.Delete();
            foreach (DirectoryInfo dir in di.GetDirectories())
                if (arguments.Remove.Contains(dir.Name))
                    dir.Delete(true);
        }

    }

    public static bool OpenExe(IProgress<float>? progress, ProgressBar main, ProgressBarOptions options, ArgOptions arguments)
    {
        using var child = main.Spawn(100, $"updated successfully. opening '{arguments.OpenExe}' to {arguments.FilePath}", options);
        try
        {
            progress = child.AsProgress<float>();
            progress.Report(100);

            Process.Start(arguments.FilePath + arguments.OpenExe);

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
