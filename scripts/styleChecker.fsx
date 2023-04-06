#!/usr/bin/env -S dotnet fsi

#r "nuget: Mono.Unix, Version=7.1.0-final.1.21458.1"
#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"
#load "../src/FileConventions/Helpers.fs"

open System
open System.IO

open Fsdk
open Fsdk.Process

open Helpers

let fantomlessToolVersion = "4.7.997-prerelease"
let prettierVersion = "2.8.3"
let pluginXmlVersion = "v2.2.0"

let InstallFantomlessTool(version: string) =
    let isFantomlessInstalled =
        let installedPackages: string =
            Process
                .Execute(
                    {
                        Command = "dotnet"
                        Arguments = "tool list"
                    },
                    Echo.Off
                )
                .UnwrapDefault()

        installedPackages.Split Environment.NewLine
        |> Seq.map(fun line ->
            line.Contains "fantomless-tool"
            && line.Contains fantomlessToolVersion
        )
        |> Seq.contains true

    if not(isFantomlessInstalled) then
        Process
            .Execute(
                {
                    Command = "dotnet"
                    Arguments = "new tool-manifest --force"
                },
                Echo.Off
            )
            .UnwrapDefault()
        |> ignore

        Process
            .Execute(
                {
                    Command = "dotnet"
                    Arguments =
                        $"tool install fantomless-tool --version {version}"
                },
                Echo.Off
            )
            .UnwrapDefault()
        |> ignore

    Process
        .Execute(
            {
                Command = "dotnet"
                Arguments = "tool restore"
            },
            Echo.Off
        )
        .UnwrapDefault()
    |> ignore

let UnwrapProcessResult
    (suggestion: string)
    (raiseError: bool)
    (processResult: ProcessResult)
    : string =
    let errMsg =
        sprintf
            "Error when running '%s %s'"
            processResult.Details.Command
            processResult.Details.Args

    match processResult.Result with
    | Success output ->
        Console.WriteLine output
        output
    | Error(_, output) ->
        if processResult.Details.Echo = Echo.Off then
            output.PrintToConsole()
            Console.WriteLine()
            Console.Out.Flush()

        let fullErrMsg = errMsg + Environment.NewLine + suggestion
        Console.Error.WriteLine fullErrMsg

        if raiseError then
            raise <| ProcessFailed errMsg
        else
            fullErrMsg
    | WarningsOrAmbiguous output ->
        if processResult.Details.Echo = Echo.Off then
            output.PrintToConsole()
            Console.WriteLine()
            Console.Out.Flush()

        let fullErrMsg = sprintf "%s (with warnings?)" errMsg
        Console.Error.WriteLine fullErrMsg
        fullErrMsg

let IsProcessSuccessful(processResult: ProcessResult) : bool =
    match processResult.Result with
    | Success output -> true
    | _ -> false

let InstallPrettier(version: string) =
    let isPrettierInstalled =
        Process.Execute(
            {
                Command = "npm"
                Arguments = $"list prettier@{version}"
            },
            Echo.All
        )
        |> IsProcessSuccessful

    if not(isPrettierInstalled) then
        Process.Execute(
            {
                Command = "npm"
                Arguments = $"install prettier@{version}"
            },
            Echo.Off
        )
        |> UnwrapProcessResult "" true
        |> ignore

let StyleFSharpFiles(rootDir: DirectoryInfo) =
    InstallFantomlessTool(fantomlessToolVersion)

    Process
        .Execute(
            {
                Command = "dotnet"
                Arguments = $"fantomless --recurse {rootDir.FullName}"
            },
            Echo.Off
        )
        .UnwrapDefault()
    |> ignore

let StyleCSharpFiles(rootDir: DirectoryInfo) =
    Process
        .Execute(
            {
                Command = "dotnet"
                Arguments = $"format whitespace {rootDir.FullName} --folder"
            },
            Echo.Off
        )
        .UnwrapDefault()
    |> ignore

let InstallPrettierPluginXml(version: string) =
    let isPrettierPluginXmlInstalled =
        Process.Execute(
            {
                Command = "npm"
                Arguments = $"list @prettier/plugin-xml@{version}"
            },
            Echo.Off
        )
        |> IsProcessSuccessful

    if not(isPrettierPluginXmlInstalled) then
        Process
            .Execute(
                {
                    Command = "npm"
                    Arguments = $"install @prettier/plugin-xml@{version}"
                },
                Echo.Off
            )
            .UnwrapDefault()
        |> ignore

let StyleXamlFiles() =
    InstallPrettier(prettierVersion)
    InstallPrettierPluginXml(pluginXmlVersion)

    Process
        .Execute(
            {
                Command = "npm"
                Arguments =
                    $"install --save-dev prettier@{prettierVersion} @prettier/plugin-xml@{pluginXmlVersion}"
            },
            Echo.Off
        )
        .UnwrapDefault()
    |> ignore

    Process
        .Execute(
            {
                Command = "./node_modules/.bin/prettier"
                Arguments =
                    "--xml-whitespace-sensitivity ignore --tab-width 4 --prose-wrap preserve --write '**/*.xaml'"
            },
            Echo.Off
        )
        .UnwrapDefault()
    |> ignore

let RunPrettier(arguments: string) =

    // We need this step so we can change the files using `npx prettier --write` in the next step.
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

    Process.Execute(
        {
            Command = "npx"
            Arguments = $"prettier {arguments}"
        },
        Echo.Off
    )
    |> UnwrapProcessResult "" true
    |> ignore


    // Since after installing commitlint dependencies package.json file changes, we need to
    // run the following command to ignore package.json file
    Process
        .Execute(
            {
                Command = "git"
                Arguments = "restore package.json"
            },
            Echo.Off
        )
        .UnwrapDefault()
    |> ignore

let StyleTypeScriptFiles() =
    RunPrettier "--quote-props=consistent --write ./**/*.ts"

let StyleYmlFiles() =
    RunPrettier "--quote-props=consistent --write ./**/*.yml"

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
        "Please style your F# code using: `dotnet fantomless --recurse .`"

    GitRestore()

    let success =
        if ContainsFiles rootDir "*.fs" || ContainsFiles rootDir ".fsx" then
            StyleFSharpFiles rootDir
            let processResult = GitDiff()
            UnwrapProcessResult suggestion false processResult |> ignore
            IsProcessSuccessful processResult

        else
            true

    success

let CheckStyleOfTypeScriptFiles(rootDir: DirectoryInfo) : bool =
    let suggestion =
        "Please style your TypeScript code using: `npx prettier --quote-props=consistent --write ./**/*.ts`"

    GitRestore()

    let success =
        if ContainsFiles rootDir "*.ts" then
            InstallPrettier prettierVersion
            StyleTypeScriptFiles()
            let processResult = GitDiff()
            UnwrapProcessResult suggestion false processResult |> ignore
            IsProcessSuccessful processResult

        else
            true

    success

let CheckStyleOfYmlFiles(rootDir: DirectoryInfo) : bool =
    let suggestion =
        "Please style your YML code using: `npx prettier --quote-props=consistent --write ./**/*.yml`"

    GitRestore()

    let success =
        if ContainsFiles rootDir "*.yml" then
            InstallPrettier prettierVersion
            StyleYmlFiles()
            let processResult = GitDiff()
            UnwrapProcessResult suggestion false processResult |> ignore
            IsProcessSuccessful processResult
        else
            true

    success

let CheckStyleOfCSharpFiles(rootDir: DirectoryInfo) : bool =
    let suggestion =
        "Please style your C# code using: `dotnet format whitespace . --folder"

    GitRestore()

    let success =
        if ContainsFiles rootDir "*.cs" then
            StyleCSharpFiles rootDir
            let processResult = GitDiff()
            UnwrapProcessResult suggestion false processResult |> ignore
            IsProcessSuccessful processResult
        else
            true

    success

let CheckStyleOfXamlFiles(rootDir: DirectoryInfo) : bool =
    let suggestion =
        "Please style your XAML code using:"
        + Environment.NewLine
        + "`./node_modules/.bin/prettier --xml-whitespace-sensitivity ignore --tab-width 4 --prose-wrap preserve --write '**/*.xaml`"

    GitRestore()

    let success =
        if ContainsFiles rootDir "*.xaml" then
            StyleXamlFiles()
            let processResult = GitDiff()
            UnwrapProcessResult suggestion false processResult |> ignore
            IsProcessSuccessful processResult
        else
            true

    success

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo

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
