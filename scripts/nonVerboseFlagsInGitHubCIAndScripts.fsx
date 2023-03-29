#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "nuget: Mono.Unix, Version=7.1.0-final.1.21458.1"
#r "nuget: YamlDotNet, Version=13.0.2"
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
            FileConventions.NonVerboseFlags
    )
    |> Seq.concat

let message = "Please don't use non-verbose flags in the following files:"

Helpers.AssertNoInvalidFiles invalidFiles message
