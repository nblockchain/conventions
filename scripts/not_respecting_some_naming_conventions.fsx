#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "nuget: Mono.Unix, Version=7.1.0-final.1.21458.1"
#r "nuget: YamlDotNet, Version=13.0.2"
#load "../src/FileConventions/Library.fs"
#load "../src/FileConventions/Helpers.fs"

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo

let invalidYmlFiles =
    Helpers.GetInvalidFiles
        rootDir
        "*.yml"
        FileConventions.DetectNotUsingKebabCaseInGitHubCIJobs

Helpers.AssertNoInvalidFiles
    invalidYmlFiles
    "Please use kebab-case for CI job names in the following files:"

let scriptExtensions =
    seq {
        ".fsx"
        ".bat"
        ".sh"
    }

let scriptsWithInvalidNames =
    scriptExtensions
    |> Seq.map(fun extension ->
        Helpers.GetInvalidFiles
            rootDir
            ("*" + extension)
            FileConventions.DetectNotUsingSnakeCaseInScriptName
    )
    |> Seq.concat

Helpers.AssertNoInvalidFiles
    scriptsWithInvalidNames
    "Please use snake_case for naming the following scripts:"
