# 📦 Updater ~ [![Release](https://img.shields.io/badge/releases-orange)](https://github.com/danijerez/updater/releases) [![Release](https://img.shields.io/badge/dotnet-7.0-purple)](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

## Simple solution to update apps. Call `updater.exe` with whatever arguments you need.

# 📚 Arguments
*  `{url}`: address where the compressed file (zip) to download is located. is required and must be the first argument
*  `-z {zipname}`: name with which the downloaded file will be saved, `default`: update.zip
*  `-o {exe}`: executable to start when the process terminate, it's `optional`
*  `-r {remove}`: delete files before opening, it's `optional`
*  `-i {ignore}`: ignore files when unzipping, it's `optional`
*  `-c`: prevent the app from auto closing, is `optional`, if you run it from a terminal it is not necessary

```mermaid
stateDiagram-v2
    direction LR
    updater --> download
    download --> unzip
    unzip --> open
    
    note right of updater
        run the updater from a command line 
        with the arguments
    end note

    note left of download
        It will show the download {url} process 
        and the time taken
    end note
    
    note right of unzip
        unzip {zipname} and overwrite the downloaded 
        file in the directory
    end note
    
    note left of open
        run the executable {exe} and close (-c) 
        the terminal
    end note
```

# ▶️ How to start 
### _in your favorite terminal_
```
.\updater.exe 'url' -f 'zipname' -o 'exe' -c
```
### _example_
```
.\updater.exe https://github.com/NickeManarin/ScreenToGif/releases/download/2.37.1/ScreenToGif.2.37.1.Portable.x64.zip -z test.zip -o ScreenToGif.exe -c
```

<img src="imgs/sample.gif" width=800px> 

# 🦄 Nugets
| Name        | Descripción | Version     |
| ----------- | ----------- | ----------- |
| [Serilog](https://github.com/saeidjoker/libc.translation/)   | Simple .NET logging with fully-structured events                                            |2.12.0|
| [DotNetZip](https://github.com/saeidjoker/libc.translation/)   | .NET library for handling ZIP files, and some associated tools.                                            |1.16.0|
| [ShellProgressBar](https://github.com/saeidjoker/libc.translation/)   | visualize (concurrent) progress in your console application    |5.2.0|
