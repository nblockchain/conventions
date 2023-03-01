#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"

Fsdk
    .Process
    .Execute(
        {
            Command = "dotnet"
            Arguments = sprintf "new tool-manifest"
        },
        Fsdk.Process.Echo.All
    )
    .UnwrapDefault()
|> ignore<string>

// we need to install specific version because of this bug: https://github.com/dotnet/sdk/issues/24037
Fsdk
    .Process
    .Execute(
        {
            Command = "dotnet"
            Arguments = sprintf "tool install fsxc --version 0.5.9.1"
        },
        Fsdk.Process.Echo.All
    )
    .UnwrapDefault()
|> ignore<string>

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo

Directory.GetFiles(rootDir.FullName, "*.fsx", SearchOption.AllDirectories)
|> Seq.iter(fun filePath ->
    Fsdk
        .Process
        .Execute(
            {
                Command = "dotnet"
                Arguments = sprintf "fsxc %s" filePath
            },
            Fsdk.Process.Echo.All
        )
        .UnwrapDefault()
    |> ignore<string>
)
