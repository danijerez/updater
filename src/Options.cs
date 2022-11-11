using CommandLine;

public class Options
{
    [Value(0, Required = true)]
    public string? DownloadUrl { get; set; }

    [Option('z', "namezip")]
    public string? NameZip { get; set; }

    [Option('p', "filepath")]
    public string? FilePath { get; set; }

    [Option('c', "close")]
    public bool AutoClose { get; set; }

    [Option('o', "open")]
    public string? OpenExe { get; set; }

    [Option('r', "remove", Separator = ',')]
    public IEnumerable<string>? Remove { get; set; }

    [Option('i', "ignore", Separator = ',')]
    public IEnumerable<string>? Ignore { get; set; }

}

