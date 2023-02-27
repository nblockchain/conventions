module Helpers

open System
open System.IO

let NotInDir (dirName: string) (fileInfo: FileInfo) =
    not(
        fileInfo.FullName.Contains
            $"%c{Path.DirectorySeparatorChar}%s{dirName}%c{Path.DirectorySeparatorChar}"
    )

let GetInvalidFiles
    (rootDirectory: DirectoryInfo)
    (searchPattern: string)
    filterFunction
    =
    Directory.GetFiles(
        rootDirectory.FullName,
        searchPattern,
        SearchOption.AllDirectories
    )
    |> Seq.map(fun pathStr -> FileInfo pathStr)
    |> Seq.filter(NotInDir "node_modules")
    |> Seq.filter(NotInDir ".git")
    |> Seq.filter(NotInDir "bin")
    |> Seq.filter(NotInDir "obj")
    |> Seq.filter(NotInDir "DummyFiles")
    |> Seq.filter filterFunction

let AssertNoInvalidFiles (invalidFiles: seq<FileInfo>) (message: string) =
    if Seq.length invalidFiles > 0 then
        let message =
            message
            + Environment.NewLine
            + (invalidFiles
               |> Seq.map(fun fileInfo -> fileInfo.FullName)
               |> String.concat Environment.NewLine)

        failwith message
