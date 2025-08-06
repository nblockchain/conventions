#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"

open Fsdk

#load "../src/FileConventions/Helpers.fs"

let version = "4.7.997-prerelease"

let args = Fsdk.Misc.FsxOnlyArguments()

match args with
| [] -> ()
| _ ->
    failwithf
        "Too many arguments (this script doesn't receive any arguments): %A"
        args

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo
let currentDir = Directory.GetCurrentDirectory() |> DirectoryInfo

let targetDir, _ = Helpers.PreferLessDeeplyNestedDir currentDir rootDir

let formattingConfigFileName = ".editorconfig"

let formattingDefaultConfigFile =
    Path.Combine(rootDir.FullName, formattingConfigFileName) |> FileInfo

let formattingConfigFileInTargetDir =
    Path.Combine(targetDir.FullName, formattingConfigFileName) |> FileInfo

if formattingDefaultConfigFile.Exists
   && not formattingConfigFileInTargetDir.Exists then
    formattingDefaultConfigFile.CopyTo(
        formattingConfigFileInTargetDir.FullName,
        false
    )
    |> ignore<FileInfo>

let formattingConfigFileInCurrentDir =
    Path.Combine(currentDir.FullName, formattingConfigFileName) |> FileInfo

if formattingDefaultConfigFile.Exists
   && not formattingConfigFileInCurrentDir.Exists then
    formattingDefaultConfigFile.CopyTo(
        formattingConfigFileInCurrentDir.FullName,
        false
    )
    |> ignore<FileInfo>

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

Fsdk
    .Process
    .Execute(
        {
            Command = "dotnet"
            Arguments =
                sprintf "tool install fantomless-tool --version %s" version
        },
        Fsdk.Process.Echo.All
    )
    .UnwrapDefault()
|> ignore<string>

Fsdk
    .Process
    .Execute(
        {
            Command = "dotnet"
            Arguments = sprintf "fantomless --recurse %s" targetDir.FullName
        },
        Fsdk.Process.Echo.All
    )
    .UnwrapDefault()
|> ignore<string>

// If this is run in a GitHub Actions environment, check for changes
if
    not
        (
            String.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable "GITHUB_EVENT_PATH"
            )
        )
then
    Fsdk
        .Process
        .Execute(
            {
                Command = "git"
                Arguments = "diff --exit-code"
            },
            Fsdk.Process.Echo.All
        )
        .UnwrapDefault()
    |> ignore<string>
