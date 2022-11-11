﻿using CommandLine;

public class Options
{
    [Value(0, Required = true, HelpText = "Address where the compressed file (zip) to download is located. is required and must be the first argument")]
    public string? DownloadUrl { get; set; }

    [Option('z', "zipname", HelpText = "Name with which the downloaded file will be saved, default: update.zip")]
    public string? NameZip { get; set; }

    [Option('p', "filepath", HelpText = "Path where to download and unpack, default is base directory")]
    public string? FilePath { get; set; }

    [Option('w', "wait", HelpText = "Prevent the app from auto closing")]
    public bool AutoClose { get; set; }

    [Option('o', "open", HelpText = "Executable to start when the process terminate. Separator ','")]
    public string? OpenExe { get; set; }

    [Option('r', "remove", Separator = ',', HelpText = "Remove files before opening. Separator ','")]
    public IEnumerable<string>? Remove { get; set; }

    [Option('i', "ignore", Separator = ',', HelpText = "Ignore files when unzipping")]
    public IEnumerable<string>? Ignore { get; set; }

}
