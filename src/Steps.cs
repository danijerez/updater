using Ionic.Zip;
using Serilog;
using ShellProgressBar;
using System.Diagnostics;

public static class Steps
{
    public static async Task<bool> DownloadFile(IProgress<float>? progress, ProgressBar main, ProgressBarOptions options, Options arguments)
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

    public static bool UnpackDownload(IProgress<float>? progress, ProgressBar main, ProgressBarOptions options, Options arguments)
    {
        using var child = main.Spawn(0, $"unpack '{arguments.FileName}.{arguments.FileExtension}' in '{arguments.FilePath}'", options);

        try
        {

            if (arguments.FileExtension.Equals(PackFormat.msi))
            {
                Process process = new Process();
                process.StartInfo.WorkingDirectory = arguments.FilePath;
                process.StartInfo.FileName = "msiexec";
                process.StartInfo.Verb = "runas";
                process.StartInfo.Arguments = $"/i {arguments.FilePath}{arguments.FileName} /quiet /qn /norestart ALLUSERS=1";

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = false;
                process.Start();
                process.WaitForExit();
                child.Tick();
                main.Tick();
                return true;
            }

            if (arguments.FileExtension.Equals(PackFormat.zip))
            {
                progress = child.AsProgress<float>();
                using (ZipFile archive = new ZipFile(arguments.FilePath + arguments.FileName))
                {
                    if (arguments.Ignore != null)
                        foreach (var i in arguments.Ignore)
                            archive.RemoveSelectedEntries(i);

                    if (arguments.FilePath == null) return false;

                    archive.ExtractProgress += new EventHandler<ExtractProgressEventArgs>((sender, e) => ExtractProgress(sender, e, progress, child, arguments.FilePath));
                    archive.ExtractAll(arguments.FilePath, ExtractExistingFileAction.OverwriteSilently);
                }
                child.Message = $"unzip successfully '{arguments.FileName}' in '{arguments.FilePath}'";
                child.Tick();
                main.Tick();
                return true;
            }

            return false;

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

    public static void RemoveFilesOrDirectory(Options arguments)
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


    public static bool OpenExe(IProgress<float>? progress, ProgressBar main, ProgressBarOptions options, Options arguments)
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
