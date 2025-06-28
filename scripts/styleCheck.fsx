#!/usr/bin/env -S dotnet fsi

#r "nuget: Mono.Unix, Version=7.1.0-final.1.21458.1"
#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"
#r "nuget: YamlDotNet, Version=16.1.3"
#load "../src/FileConventions/Config.fs"
#load "../src/FileConventions/Helpers.fs"
#load "../src/FileConventions/Library.fs"

open System
open System.IO

open Fsdk
open Fsdk.Process

open Helpers
open FileConventions

let ContainsFiles (rootDir: DirectoryInfo) (searchPattern: string) =
    Helpers.GetFiles rootDir searchPattern |> Seq.length > 0

let GitDiff() : ProcessResult =

    // Since we changed file modes in the prettier step we need the following command to
    // make git ignore mode changes in files and doesn't include them in the git diff command.
    Process
        .Execute(
            {
                Command = "git"
                Arguments = "config core.fileMode false"
            },
            Echo.Off
        )
        .UnwrapDefault()
    |> ignore

    let processResult =
        Process.Execute(
            {
                Command = "git"
                Arguments = "diff --exit-code"
            },
            Echo.Off
        )

    processResult

let GitRestore() =
    Process
        .Execute(
            {
                Command = "git"
                Arguments = "restore ."
            },
            Echo.Off
        )
        .UnwrapDefault()
    |> ignore

let CheckStyleOfFSharpFiles(rootDir: DirectoryInfo) : bool =
    let suggestion =
        Some "Please style your F# code using: `dotnet fantomless --recurse .`"

    GitRestore()

    let success =
        if ContainsFiles rootDir "*.fs" || ContainsFiles rootDir ".fsx" then
            StyleFSharpFiles rootDir
            let processResult = GitDiff()
            UnwrapProcessResult suggestion true processResult |> ignore
            IsProcessSuccessful processResult

        else
            true

    success

let CheckStyleOfTypeScriptFiles(rootDir: DirectoryInfo) : bool =
    let pattern =
        $".{Path.DirectorySeparatorChar}**{Path.DirectorySeparatorChar}*.ts"

    let suggestion =
        Some
            $"Please style your TypeScript code using: `npx prettier --quote-props=consistent --write {pattern}`"

    GitRestore()

    let success =
        if ContainsFiles rootDir "*.ts" then
            StyleTypeScriptFiles()
            let processResult = GitDiff()
            UnwrapProcessResult suggestion true processResult |> ignore
            IsProcessSuccessful processResult

        else
            true

    success

let CheckStyleOfYmlFiles(rootDir: DirectoryInfo) : bool =
    let pattern =
        $".{Path.DirectorySeparatorChar}**{Path.DirectorySeparatorChar}*.yml"

    let suggestion =
        Some
            $"Please style your YML code using: `npx prettier --quote-props=consistent --write {pattern}`"

    GitRestore()

    let success =
        if ContainsFiles rootDir "*.yml" then
            StyleYmlFiles()
            let processResult = GitDiff()
            UnwrapProcessResult suggestion true processResult |> ignore
            IsProcessSuccessful processResult
        else
            true

    success

let CheckStyleOfCSharpFiles(rootDir: DirectoryInfo) : bool =
    let suggestion =
        Some
            "Please style your C# code using: `dotnet format whitespace . --folder"

    GitRestore()

    let success =
        if ContainsFiles rootDir "*.cs" then
            StyleCSharpFiles rootDir
            let processResult = GitDiff()
            UnwrapProcessResult suggestion true processResult |> ignore
            IsProcessSuccessful processResult
        else
            true

    success

let CheckStyleOfXamlFiles(rootDir: DirectoryInfo) : bool =
    let prettierPath = Path.Combine(".", "node_modules", ".bin", "prettier")

    let pattern = $"**{Path.DirectorySeparatorChar}*.xaml"

    let suggestion =
        "Please style your XAML code using:"
        + Environment.NewLine
        + $"`{prettierPath} --xml-whitespace-sensitivity ignore --tab-width 4 --prose-wrap preserve --write {pattern}`"
        |> Some

    GitRestore()

    let success =
        if ContainsFiles rootDir "*.xaml" then
            StyleXamlFiles()
            let processResult = GitDiff()
            UnwrapProcessResult suggestion true processResult |> ignore
            IsProcessSuccessful processResult
        else
            true

    success

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo

// We need this step so we can change the files using `npx prettier --write` in the prettier calls.
// Otherwise we get permission denied error in the CI.
Process
    .Execute(
        {
            Command = "chmod"
            Arguments = "777 --recursive ."
        },
        Echo.Off
    )
    .UnwrapDefault()
|> ignore

let processSuccessStates =
    [|
        CheckStyleOfFSharpFiles rootDir
        CheckStyleOfCSharpFiles rootDir
        CheckStyleOfTypeScriptFiles rootDir
        CheckStyleOfYmlFiles rootDir
        CheckStyleOfXamlFiles rootDir
    |]

if processSuccessStates |> Seq.contains false then
    Environment.Exit 1
