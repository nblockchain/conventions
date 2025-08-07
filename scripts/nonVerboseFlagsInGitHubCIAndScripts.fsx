#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "nuget: Mono.Unix, Version=7.1.0-final.1.21458.1"
#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"
#r "nuget: YamlDotNet, Version=16.1.3"

#load "../src/FileConventions/Config.fs"
#load "../src/FileConventions/Helpers.fs"
#load "../src/FileConventions/Library.fs"

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
            FileConventions.NonVerboseFlags
    )
    |> Seq.concat

let message = "Please don't use non-verbose flags in the following files:"

Helpers.AssertNoInvalidFiles invalidFiles message
