#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "nuget: Fsdk, Version=0.6.0--date20230821-0702.git-5488853"
#load "../src/FileConventions/Helpers.fs"

Fsdk
    .Process
    .Execute(
        {
            Command = "dotnet"
            Arguments = sprintf "new tool-manifest --force"
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

Helpers.GetFiles rootDir "*.fsx"
|> Seq.iter(fun fileInfo ->
    Fsdk
        .Process
        .Execute(
            {
                Command = "dotnet"
                Arguments = sprintf "fsxc %s" fileInfo.FullName
            },
            Fsdk.Process.Echo.All
        )
        .UnwrapDefault()
    |> ignore<string>
)
