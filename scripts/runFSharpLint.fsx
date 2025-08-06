#!/usr/bin/env -S dotnet fsi

open System.IO
open System.Linq

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"

open Fsdk

#load "../src/FileConventions/Helpers.fs"

let version = "0.26.1--date20250724-0123.git-3924085"

let targetSol =
    let args = Fsdk.Misc.FsxOnlyArguments()

    match args with
    | [] -> None
    | head :: [] -> Some head
    | head :: tail -> failwithf "Too many arguments: %A" args

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo
let currentDir = Directory.GetCurrentDirectory() |> DirectoryInfo

let targetDir, fallbackDir =
    Helpers.PreferLessDeeplyNestedDir currentDir rootDir

let targetSolution =
    match targetSol with
    | Some solFilename ->
        let solFile = Path.Combine(targetDir.FullName, solFilename) |> FileInfo

        if not solFile.Exists then
            let solFile =
                Path.Combine(fallbackDir.FullName, solFilename) |> FileInfo

            if not solFile.Exists then
                failwithf
                    "Solution file %s not found in %s or %s"
                    solFilename
                    targetDir.FullName
                    fallbackDir.FullName
            else
                printfn
                    "Using solution file %s from fallback directory %s"
                    solFilename
                    fallbackDir.FullName

                solFile
        else
            printfn
                "Using solution file %s from target directory %s"
                solFilename
                targetDir.FullName

            solFile
    | None ->
        let solFiles = Directory.GetFiles(targetDir.FullName, "*.sln")

        if solFiles.Length = 0 then
            let solFiles = Directory.GetFiles(fallbackDir.FullName, "*.sln")

            if solFiles.Length = 0 then
                failwithf
                    "No solution file found in %s or %s"
                    targetDir.FullName
                    fallbackDir.FullName
            elif solFiles.Length > 1 then
                failwithf
                    "Multiple solution files found in %s; please specify one as an argument"
                    fallbackDir.FullName
            else
                let solFile = solFiles.[0] |> FileInfo

                printfn
                    "Using solution file %s from target directory %s"
                    solFile.Name
                    fallbackDir.FullName

                solFile
        elif solFiles.Length > 1 then
            failwithf
                "Multiple solution files found in %s; please specify one as an argument"
                targetDir.FullName
        else
            let solFile = solFiles.[0] |> FileInfo

            printfn
                "Using solution file %s from target directory %s"
                solFile.Name
                targetDir.FullName

            solFile

let fsharpLintConfigFileName = "fsharplint.json"

let fsharpLintDefaultConfigFile =
    Path.Combine(rootDir.FullName, fsharpLintConfigFileName) |> FileInfo

let fsharpLintConfigFile =
    Path.Combine(targetSolution.Directory.FullName, fsharpLintConfigFileName)
    |> FileInfo

if fsharpLintDefaultConfigFile.Exists && not fsharpLintConfigFile.Exists then
    fsharpLintDefaultConfigFile.CopyTo(fsharpLintConfigFile.FullName, false)
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
                sprintf "tool install dotnet-fsharplint --version %s" version
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
            Arguments = sprintf "build %s" targetSolution.FullName
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
                sprintf "dotnet-fsharplint lint %s" targetSolution.FullName
        },
        Fsdk.Process.Echo.All
    )
    .UnwrapDefault()
|> ignore<string>
