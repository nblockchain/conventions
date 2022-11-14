module FileConventions

open System.Linq
open System.IO
open System
open Mono.Posix
open Mono.Unix
open Mono.Unix.Native

let HasCorrectShebang (fileInfo: FileInfo) =
    let fileText = File.ReadLines(fileInfo.FullName)
    if fileText.Any() then
        let firstLine = fileText.First()
        (
            firstLine.StartsWith("#!/usr/bin/env fsx") || 
            firstLine.StartsWith("#!/usr/bin/env -S dotnet fsi")
        )
    else
        false


let IsExecutable (fileInfo: FileInfo) =
    let hasExecuteAccess = Syscall.access(fileInfo.FullName, AccessModes.X_OK)
    hasExecuteAccess

