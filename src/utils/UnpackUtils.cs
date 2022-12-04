using SharpCompress.Archives;
using SharpCompress.Common;
using ShellProgressBar;
using System.Diagnostics;

namespace updater.utils
{
    public static class UnpackUtils
    {
        public static bool UnpackGeneric(IProgress<float>? progress, IEnumerable<IArchiveEntry> entries, ArgOptions arguments, ChildProgressBar child, ProgressBar main)
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
    
        public static bool UnpackMsi(ArgOptions arguments, ChildProgressBar child, ProgressBar main)
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
    }
}
