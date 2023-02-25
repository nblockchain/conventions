#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#load "../src/FileConventions/Library.fs"

let NotInDir (dirName: string) (fileInfo: FileInfo) =
    not(
        fileInfo.FullName.Contains
            $"%c{Path.DirectorySeparatorChar}%s{dirName}%c{Path.DirectorySeparatorChar}"
    )

let invalidFiles =
    Directory.GetFiles(".", "*.yml", SearchOption.AllDirectories)
    |> Seq.map(fun pathStr -> FileInfo pathStr)
    |> Seq.filter(NotInDir "node_modules")
    |> Seq.filter(NotInDir ".git")
    |> Seq.filter(NotInDir "bin")
    |> Seq.filter(NotInDir "obj")
    |> Seq.filter(NotInDir "DummyFiles")
    |> Seq.filter(fun fileInfo ->
        FileConventions.DetectUnpinnedVersionsInGitHubCI fileInfo
    )

if Seq.length invalidFiles > 0 then
    let message =
        "The following files shouldn't contain -latest in runs-on: GitHubCI tags:"
        + Environment.NewLine
        + (invalidFiles
           |> Seq.map(fun fileInfo -> fileInfo.FullName)
           |> String.concat Environment.NewLine)
        + Environment.NewLine
        + "Here is a list of available runner image versions that you can use:"
        + Environment.NewLine
        + "https://github.com/actions/runner-images#available-images"

    failwith message
