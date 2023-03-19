using SharpCompress.Archives;
using SharpCompress.Common;
using ShellProgressBar;
using System.Diagnostics;
using updater.models;

namespace updater.utils
{
    public static class UnpackUtils
    {
        public static bool UnpackGeneric(IProgress<float>? progress, IEnumerable<IArchiveEntry> entries, ArgOptions arguments, ChildProgressBar child, ProgressBar main)
        {
            if (arguments.FilePath == null)
                return false;

            child.MaxTicks = entries.Count();

            foreach (var entry in entries)
            {
                if (arguments.Ignore == null || !arguments.Ignore.Contains(entry.Key))
                {

                    child.Message = $"decompress {entry.Archive.Type} '{entry.Key}' in '{arguments.FilePath}'";
                    entry.WriteToDirectory(arguments.FilePath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });


                }

                child.Tick();
            }

            child.Message = $"decompress successfully '{arguments.FileName}' in '{arguments.FilePath}'";
            child.Tick();
            main.Tick();
            return true;
        }

        public static bool UnpackMsi(ArgOptions arguments, ChildProgressBar child, ProgressBar main)
        {
            Process process = new Process();
            process.StartInfo.WorkingDirectory = arguments.FilePath;
            process.StartInfo.FileName = "msiexec";
            process.StartInfo.Verb = "runas";
            process.StartInfo.Arguments = $"/package {arguments.FilePath}{arguments.FileName} /quiet";

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = false;
            process.Start();
            process.WaitForExit();
            child.Tick();
            main.Tick();
            return true;
        }
    }
}
