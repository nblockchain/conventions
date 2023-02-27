#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#load "../src/FileConventions/Library.fs"
#load "../src/FileConventions/Helpers.fs"

let rootDir =
    Path.Combine(__SOURCE_DIRECTORY__, "..")
    |> DirectoryInfo

let invalidFiles =
    Helpers.GetInvalidFiles
        rootDir
        "*.yml"
        FileConventions.DetectAsteriskInPackageReferenceItems

let message =
    "The following files shouldn't use asterisk (*) in PackageReference items of .NET projects."
    + Environment.NewLine
    + "Please use the exact version of the package instead."

Helpers.AssertNoInvalidFiles invalidFiles message
