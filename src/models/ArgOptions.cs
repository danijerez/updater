﻿using CommandLine;
namespace updater.models
{
    public class ArgOptions
    {
        [Value(0, Required = true, HelpText = "Address where the compressed file (zip) to download is located. is required and must be the first argument")]
        public string? DownloadUrl { get; set; }

        [Option('f', "file", HelpText = "Name with which the downloaded file will be saved, default: update")]
        public string? FileName { get; set; }

        [Option('e', "extension", HelpText = "Only use in special cases like 'msi', by default it autodetects, supported Archive Formats: Zip, GZip, Tar, Rar, 7Zip")]
        public PackFormat? FileExtension { get; set; }

        [Option('p', "filepath", HelpText = "Path where to download and unpack, default is base directory")]
        public string? FilePath { get; set; }

        [Option('w', "wait", HelpText = "Prevent the app from auto closing")]
        public bool WaitClose { get; set; }

        [Option('o', "open", HelpText = "Executable to start when the process terminate. Separator ','")]
        public string? OpenExe { get; set; }

        [Option('r', "remove", Separator = ',', HelpText = "Remove files before opening. Separator ','")]
        public IEnumerable<string>? Remove { get; set; }

        [Option('i', "ignore", Separator = ',', HelpText = "Ignore files when unzipping")]
        public IEnumerable<string>? Ignore { get; set; }

        [Option('d', "decopass", HelpText = "Password required to decompression the file")]
        public string? Password { get; set; }
    }

    public enum PackFormat
    {
        msi
    }
}