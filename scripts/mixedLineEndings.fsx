#!/usr/bin/env -S dotnet fsi

open System.IO
open System

#load "../src/FileConventions/Library.fs"

let NotInDir (dirName: string) (fileInfo: FileInfo) =
    not (fileInfo.FullName.Contains $"%c{Path.DirectorySeparatorChar}%s{dirName}%c{Path.DirectorySeparatorChar}")

let invalidFiles = 
    Directory.GetFiles(".", "*.*", SearchOption.AllDirectories)
    |> Seq.map (fun pathStr -> FileInfo pathStr)
    |> Seq.filter (NotInDir "node_modules")
    |> Seq.filter (NotInDir ".git")
    |> Seq.filter FileConventions.MixedLineEndings

if Seq.length invalidFiles > 0 then
    let message =
        "The following files shouldn't use mixed line endings:" +
        Environment.NewLine +
        (invalidFiles
        |> Seq.map (fun fileInfo -> fileInfo.FullName)
        |> String.concat Environment.NewLine)

    failwith message
