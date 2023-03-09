#!/usr/bin/env -S dotnet fsi

open System.IO
open System

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
