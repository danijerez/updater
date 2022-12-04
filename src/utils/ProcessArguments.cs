using CommandLine;
using CommandLine.Text;
using Serilog;
using updater.models;

namespace updater.utils
{
    public static class ProcessArguments
    {
        public static ArgOptions Read(string[] args)
        {
            var a = Parser.Default.ParseArguments<ArgOptions>(args);

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

            return a.Value;
        }

        public static void Wait()
        {
            Console.WriteLine("press any key to close...");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
