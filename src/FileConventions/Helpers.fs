module Helpers

open System
open System.IO
open System.Linq

let NotInDir (dirName: string) (fileInfo: FileInfo) =
    not(
        fileInfo.FullName.Contains
            $"%c{Path.DirectorySeparatorChar}%s{dirName}%c{Path.DirectorySeparatorChar}"
    )

let GetFiles (maybeRootDirectory: DirectoryInfo) (searchPattern: string) =
    Directory.GetFiles(
        maybeRootDirectory.FullName,
        searchPattern,
        SearchOption.AllDirectories
    )
    |> Seq.map FileInfo
    |> Seq.filter(NotInDir "node_modules")
    |> Seq.filter(NotInDir ".git")
    |> Seq.filter(NotInDir "bin")
    |> Seq.filter(NotInDir "obj")
    |> Seq.filter(NotInDir "DummyFiles")

let GetInvalidFiles
    (rootDirectory: DirectoryInfo)
    (searchPattern: string)
    filterFunction
    =
    GetFiles rootDirectory searchPattern |> Seq.filter filterFunction

let PreferLessDeeplyNestedDir
    (dirAandPreferred: DirectoryInfo)
    (dirB: DirectoryInfo)
    =
    let dirSeparatorsOfDirA =
        dirAandPreferred.FullName.Count(fun char ->
            char = Path.DirectorySeparatorChar
        )

    let dirSeparatorsOfDirB =
        dirB.FullName.Count(fun char -> char = Path.DirectorySeparatorChar)

    if dirSeparatorsOfDirB > dirSeparatorsOfDirA then
        dirAandPreferred, dirB
    elif dirSeparatorsOfDirA > dirSeparatorsOfDirB then
        dirB, dirAandPreferred
    else
        dirAandPreferred, dirB

let AssertNoInvalidFiles (invalidFiles: seq<FileInfo>) (message: string) =
    if Seq.length invalidFiles > 0 then
        let message =
            message
            + Environment.NewLine
            + (invalidFiles
               |> Seq.map(fun fileInfo -> fileInfo.FullName)
               |> String.concat Environment.NewLine)

        failwith message
