using Serilog;
using ShellProgressBar;

class Program
{
    static async Task Main(string[] args)
    {
        var filePath = AppDomain.CurrentDomain.BaseDirectory;

        Log.Logger = new LoggerConfiguration()
                     .Enrich.FromLogContext()
                     .MinimumLevel.Debug()
                     .WriteTo.File(filePath + @"\logs\Updater_.txt", rollingInterval: RollingInterval.Day)
                     .CreateLogger();

        if (args.Length < 3)
            return;

        var fileName = args[0];
        var downloadFileUrl = args[1];
        var exe = args[2];

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

        using (var main = new ProgressBar(3, $"update process '{exe}'", options))
        {
            try
            {
                var step1 = await Steps.DownloadFile(progress, main, options, downloadFileUrl, fileName);
                if (!step1) return;
                var step2 = Steps.UnZipFile(progress, main, options, fileName, filePath);
                if (!step2) return;
                var step3 = Steps.OpenExe(progress, main, options, exe, fileName);
            }
            catch (Exception e)
            {
                main.WriteErrorLine(e.Message);
                Log.Error(e.Message, e);
            }

        }

        Environment.Exit(0);

    }

}



