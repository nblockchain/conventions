#!/usr/bin/env -S dotnet fsi

open System.IO
open System

#load "../src/FileConventions/Helpers.fs"

let EolAtEof(fileInfo: FileInfo) = 
    use streamReader = new StreamReader (fileInfo.FullName)
    let filetext = streamReader.ReadToEnd()
    
    if filetext <> String.Empty then
        Seq.last filetext = '\n'
    else
        true

let invalidFiles =
    Helpers.GetInvalidFiles "." "*.*" (fun fileInfo -> not(EolAtEof fileInfo))

Helpers.AssertNoInvalidFiles
    invalidFiles
    "The following files should end with EOL:"
