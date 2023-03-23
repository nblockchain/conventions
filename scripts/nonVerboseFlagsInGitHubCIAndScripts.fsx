#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#load "../src/FileConventions/Library.fs"
#load "../src/FileConventions/Helpers.fs"

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo

let validExtensions =
    seq {
        ".yml"
        ".fsx"
        ".fs"
        ".sh"
    }

let invalidFiles =
    validExtensions
    |> Seq.map(fun ext ->
        Helpers.GetInvalidFiles
            rootDir
            ("*" + ext)
            FileConventions.NonVerboseFlagsInGitHubCI
    )
    |> Seq.concat

let message = "Please don't use non-verbose flags in the following files:"

Helpers.AssertNoInvalidFiles invalidFiles message
