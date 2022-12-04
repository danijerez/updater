using Serilog;
using SharpCompress.Archives;
using SharpCompress.Common;
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
            else
            {
                progress = child.AsProgress<float>();
                using (var archive = ArchiveFactory.Open(arguments.FilePath + arguments.FileName))
                {
                    UnPack(progress, archive.Entries.Where(entry => !entry.IsDirectory), arguments, child, main);

                    double totalSize = archive.Entries.Where(e => !e.IsDirectory).Sum(e => e.Size);
                    long completed = 0;
                    child.MaxTicks = (int)completed;
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        if (arguments.Ignore == null || !arguments.Ignore.Contains(entry.Key))
                        {
                            child.Message = $"unzip '{entry.Key}' in '{arguments.FilePath}'";
                            entry.WriteToDirectory(arguments.FilePath, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });

                            completed += entry.Size;
                            var percentage = completed / totalSize;

                            if (progress != null)
                                progress.Report((int)percentage);
                        }

                    }
                    child.Message = $"unzip successfully '{arguments.FileName}' in '{arguments.FilePath}'";
                    child.Tick();
                    main.Tick();
                    return true;
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

    public static bool UnPack(IProgress<float>? progress, IEnumerable<IArchiveEntry> entries, Options arguments, ChildProgressBar child, ProgressBar main)
    {
        if (arguments.FilePath == null)
            return false;

        progress = child.AsProgress<float>();
        double totalSize = entries.Where(e => !e.IsDirectory).Sum(e => e.Size);
        long completed = 0;
        child.MaxTicks = (int)completed;
        foreach (var entry in entries)
        {
            if (arguments.Ignore == null || !arguments.Ignore.Contains(entry.Key))
            {
                child.Message = $"unpack '{entry.Key}' in '{arguments.FilePath}'";
                entry.WriteToDirectory(arguments.FilePath, new ExtractionOptions()
                {
                    ExtractFullPath = true,
                    Overwrite = true
                });

                completed += entry.Size;
                var percentage = completed / totalSize;

                if (progress != null)
                    progress.Report((int)percentage);
            }
        }
        child.Message = $"unpack successfully '{arguments.FileName}' in '{arguments.FilePath}'";
        child.Tick();
        main.Tick();
        return true;
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
