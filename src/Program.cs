using Serilog;
using ShellProgressBar;
using System.Reflection;
using static updater.core.UpdaterSteps;
using static updater.utils.ProcessArguments;

class Program
{
    static async Task Main(string[] args)
    {
        var a = Read(args);
        ConfigureLogger();
        var options = ConfigureProgressBar();

        var nSteps = 2;
        if (a.OpenExe != null) nSteps++;

        using (var main = new ProgressBar(nSteps, $"update process", options))
        {
            try
            {
                if (a.DownloadUrl == null || a.FileName == null || a.FilePath == null) return;
                if (!await DownloadFile(null, main, options, a)) return;
                if (!UnpackDownload(null, main, options, a)) return;
                RemoveFilesOrDirectory(a);
                if (a.OpenExe != null) OpenExe(null, main, options, a);
                if (a.WaitClose) EndWait(main);
            }
            catch (Exception e)
            {
                main.WriteErrorLine(e.Message);
                Log.Error(e.Message, e);
            }
        }
    }

    private static void ConfigureLogger()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        Log.Logger = new LoggerConfiguration()
                     .Enrich.FromLogContext()
                     .MinimumLevel.Debug()
                     .WriteTo.File(AppDomain.CurrentDomain.BaseDirectory + $@"\logs\updater_{version}_.txt", rollingInterval: RollingInterval.Day)
                     .CreateLogger();
    }

    private static ProgressBarOptions ConfigureProgressBar()
    {
        return new ProgressBarOptions
        {
            ForegroundColor = ConsoleColor.Yellow,
            ForegroundColorDone = ConsoleColor.DarkGreen,
            BackgroundColor = ConsoleColor.DarkGray,
            ForegroundColorError = ConsoleColor.Red,
            BackgroundCharacter = '\u2593',
            CollapseWhenFinished = false
        };
    }
}
