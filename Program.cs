using CommandLine;
using CommandLine.Text;
using Serilog;
using ShellProgressBar;
using static Steps;

class Program
{
    static async Task Main(string[] args)
    {

        Log.Logger = new LoggerConfiguration()
                     .Enrich.FromLogContext()
                     .MinimumLevel.Debug()
                     .WriteTo.File(AppDomain.CurrentDomain.BaseDirectory + @"\logs\Updater_.txt", rollingInterval: RollingInterval.Day)
                     .CreateLogger();


        var a = Parser.Default.ParseArguments<Options>(args);

        a.WithParsed(o =>
        {

            if (o.FilePath == null)
                o.FilePath = AppDomain.CurrentDomain.BaseDirectory;

            if (o.FileName == null)
                o.FileName = "update";

            if (o.DownloadUrl == null)
            {
                var m = "the download url must be the first ardument and cannot be empty";
                Console.WriteLine(m);
                Log.Error(m);
                Wait();
            }

        })
        .WithNotParsed(e =>
        {
            var helpText = HelpText.AutoBuild(a, h => HelpText.DefaultParsingErrorsHandler(a, h), e => e);
            Wait();
        });

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
        if (a.Value.OpenExe != null) nSteps++;

        using (var main = new ProgressBar(nSteps, $"update process", options))
        {
            try
            {
                if (a.Value.DownloadUrl == null || a.Value.FileName == null || a.Value.FilePath == null) return;
                if (!await DownloadFile(progress, main, options, a.Value)) return;
                if (!UnpackDownload(progress, main, options, a.Value)) return;
                RemoveFilesOrDirectory(a.Value);
                if (a.Value.OpenExe != null) OpenExe(progress, main, options, a.Value);
                if (a.Value.WaitClose) EndWait(main);
            }
            catch (Exception e)
            {
                main.WriteErrorLine(e.Message);
                Log.Error(e.Message, e);
            }

        }

    }



}



