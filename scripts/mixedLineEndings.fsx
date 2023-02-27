#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#load "../src/FileConventions/Library.fs"
#load "../src/FileConventions/Helpers.fs"

let invalidFiles =
    Helpers.GetInvalidFiles "." "*.*" FileConventions.MixedLineEndings

Helpers.AssertNoInvalidFiles
    invalidFiles
    "The following files shouldn't use mixed line endings:"
