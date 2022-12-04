using Serilog;
using ShellProgressBar;
using System.Reflection;
using static updater.core.UpdaterSteps;
using static updater.utils.ProcessArguments;

class Program
{
    static async Task Main(string[] args)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;

        Log.Logger = new LoggerConfiguration()
                     .Enrich.FromLogContext()
                     .MinimumLevel.Debug()
                     .WriteTo.File(AppDomain.CurrentDomain.BaseDirectory + $@"\logs\updater_{version}_.txt", rollingInterval: RollingInterval.Day)
                     .CreateLogger();

        var a = Read(args);

        IProgress<float>? progress = null;

        var options = new ProgressBarOptions
        {
            ForegroundColor = ConsoleColor.Yellow,
            ForegroundColorDone = ConsoleColor.DarkGreen,
            BackgroundColor = ConsoleColor.DarkGray,
            ForegroundColorError = ConsoleColor.Red,
            BackgroundCharacter = '\u2593',
            CollapseWhenFinished = false
        };

        var nSteps = 2;
        if (a.OpenExe != null) nSteps++;

        using (var main = new ProgressBar(nSteps, $"update process", options))
        {
            try
            {
                if (a.DownloadUrl == null || a.FileName == null || a.FilePath == null) return;
                if (!await DownloadFile(progress, main, options, a)) return;
                if (!UnpackDownload(progress, main, options, a)) return;
                RemoveFilesOrDirectory(a);
                if (a.OpenExe != null) OpenExe(progress, main, options, a);
                if (a.WaitClose) EndWait(main);
            }
            catch (Exception e)
            {
                main.WriteErrorLine(e.Message);
                Log.Error(e.Message, e);
            }

        }

    }

}



