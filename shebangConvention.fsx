#!/usr/bin/env fsx

open System
open System.IO
#load "src/FileConventions/Library.fs"

let NotInDir (dirName: string) (fileInfo: FileInfo) =
    not (fileInfo.FullName.Contains $"%c{Path.DirectorySeparatorChar}%s{dirName}%c{Path.DirectorySeparatorChar}")

let invalidFiles = 
    Directory.GetFiles(".", "*.fsx", SearchOption.AllDirectories) 
    |> Seq.map (fun pathStr -> FileInfo pathStr)
    |> Seq.filter (NotInDir "node_modules")
    |> Seq.filter (NotInDir ".git")
    |> Seq.filter (NotInDir "bin")
    |> Seq.filter (NotInDir "obj")
    |> Seq.filter (NotInDir "DummyFiles")
    |> Seq.filter (fun fileInfo -> not (FileConventions.HasCorrectShebang fileInfo))

if Seq.length invalidFiles > 0 then
    let message = 
        "The following files don't have shebang:" + 
        Environment.NewLine + 
        (invalidFiles 
        |> Seq.map (fun fileInfo -> fileInfo.FullName)
        |> String.concat Environment.NewLine)
        
    failwith message
