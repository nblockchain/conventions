#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#load "../src/FileConventions/Library.fs"
#load "../src/FileConventions/Helpers.fs"

let invalidFiles =
    Helpers.GetInvalidFiles
        "."
        "*.yml"
        FileConventions.DetectAsteriskInPackageReferenceItems

let message =
    "The following files shouldn't use asterisk (*) in PackageReference items of .NET projects."
    + Environment.NewLine
    + "Please use the exact version of the package instead."

Helpers.AssertNoInvalidFiles invalidFiles message
