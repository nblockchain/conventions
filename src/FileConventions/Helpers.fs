module Helpers

open System
open System.IO
open System.Linq

open Fsdk
open Fsdk.Process

let NotInDir (dirName: string) (fileInfo: FileInfo) =
    not(
        fileInfo.FullName.Contains
            $"%c{Path.DirectorySeparatorChar}%s{dirName}%c{Path.DirectorySeparatorChar}"
    )

let GetFiles (maybeRootDirectory: DirectoryInfo) (searchPattern: string) =
    Directory.GetFiles(
        maybeRootDirectory.FullName,
        searchPattern,
        SearchOption.AllDirectories
    )
    |> Seq.map(fun pathStr -> FileInfo pathStr)
    |> Seq.filter(NotInDir "node_modules")
    |> Seq.filter(NotInDir ".git")
    |> Seq.filter(NotInDir "bin")
    |> Seq.filter(NotInDir "obj")
    |> Seq.filter(NotInDir "DummyFiles")

let GetInvalidFiles
    (rootDirectory: DirectoryInfo)
    (searchPattern: string)
    filterFunction
    =
    GetFiles rootDirectory searchPattern |> Seq.filter filterFunction

let PreferLessDeeplyNestedDir
    (dirAandPreferred: DirectoryInfo)
    (dirB: DirectoryInfo)
    =
    let dirSeparatorsOfDirA =
        dirAandPreferred.FullName.Count(fun char ->
            char = Path.DirectorySeparatorChar
        )

    let dirSeparatorsOfDirB =
        dirB.FullName.Count(fun char -> char = Path.DirectorySeparatorChar)

    if dirSeparatorsOfDirB > dirSeparatorsOfDirA then
        dirAandPreferred
    elif dirSeparatorsOfDirA > dirSeparatorsOfDirB then
        dirB
    else
        dirAandPreferred

let AssertNoInvalidFiles (invalidFiles: seq<FileInfo>) (message: string) =
    if Seq.length invalidFiles > 0 then
        let message =
            message
            + Environment.NewLine
            + (invalidFiles
               |> Seq.map(fun fileInfo -> fileInfo.FullName)
               |> String.concat Environment.NewLine)

        failwith message

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
            line.Contains "fantomless-tool" && line.Contains version
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
    (maybeSuggestion: Option<string>)
    (ignoreErrorExitCode: bool)
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

        let fullErrMsg =
            match maybeSuggestion with
            | Some suggestion -> errMsg + Environment.NewLine + suggestion
            | None -> errMsg

        Console.Error.WriteLine fullErrMsg

        if ignoreErrorExitCode then
            fullErrMsg
        else
            raise <| ProcessFailed errMsg

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
    | Success _ -> true
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
        |> UnwrapProcessResult None false
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

let RunPrettier(arguments: string) =

    Process.Execute(
        {
            Command = "npx"
            Arguments = $"prettier {arguments}"
        },
        Echo.Off
    )
    |> UnwrapProcessResult None false
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
