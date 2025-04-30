#!/usr/bin/env -S dotnet fsi

open System.IO
open System

#r "nuget: Mono.Unix, Version=7.1.0-final.1.21458.1"
#r "nuget: YamlDotNet, Version=16.1.3"
#r "nuget: Fsdk, Version=0.6.0--date20230821-0702.git-5488853"

#load "../src/FileConventions/Helpers.fs"
#load "../src/FileConventions/Library.fs"

open type FileConventions.EolAtEof

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo

let invalidFiles =
    Helpers.GetInvalidFiles
        rootDir
        "*.*"
        (fun fileInfo -> FileConventions.EolAtEof fileInfo = False)

Helpers.AssertNoInvalidFiles
    invalidFiles
    "The following files should end with EOL:"
