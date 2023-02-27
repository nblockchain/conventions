#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#load "../src/FileConventions/Library.fs"
#load "../src/FileConventions/Helpers.fs"

let invalidFiles =
    Helpers.GetInvalidFiles
        "."
        "*.fsx"
        (fun fileInfo -> not(FileConventions.HasCorrectShebang fileInfo))

Helpers.AssertNoInvalidFiles
    invalidFiles
    "The following files should have the correct shebang:"
