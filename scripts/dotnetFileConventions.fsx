#!/usr/bin/env -S dotnet fsi

open System
open System.IO
open System.Text.RegularExpressions

#r "nuget: Mono.Unix, Version=7.1.0-final.1.21458.1"
#r "nuget: Fsdk, Version=0.6.0--date20230821-0702.git-5488853"
#r "nuget: YamlDotNet, Version=16.1.3"

open Fsdk
open Fsdk.Process

#load "../src/FileConventions/Helpers.fs"
#load "../src/FileConventions/Library.fs"

open FileConventions
open Helpers

let args = Misc.FsxOnlyArguments()

if args.Length > 1 then
    Console.Error.WriteLine
        "Usage: dotnetFileConventions.fsx [projectFolder(optional)]"

    Environment.Exit 1

let rootDir = DirectoryInfo args.[0]

// DefiningEmptyStringsWithDoubleQuotes
let allSourceFiles = ReturnAllProjectSourceFile rootDir [ "*.cs"; "*.fs" ] true
printfn "%A" (String.Join("\n", allSourceFiles))

let allProjFiles =
    ReturnAllProjectSourceFile rootDir [ "*.csproj"; "*.fsproj" ] true

for sourceFile in allSourceFiles do
    let isStringEmpty = DefiningEmptyStringsWithDoubleQuotes sourceFile

    if isStringEmpty then
        failwith(
            sprintf
                "%s file: Contains empty strings specifed with \"\" , you should use String.Empty()"
                sourceFile.FullName
        )


// ProjFilesNamingConvention

for projfile in allProjFiles do
    let isWrongProjFile = ProjFilesNamingConvention projfile

    if isWrongProjFile then
        failwith(
            sprintf
                "%s file: Project file or Project directory is incorrect!\n
        Fix: use same name on .csproj/.fsproj on parrent project directory"
                projfile.FullName
        )

// notfollowingnamespaceconvention
for sourcefile in allSourceFiles do
    let iswrongnamespace = NotFollowingNamespaceConvention sourcefile

    if iswrongnamespace then
        failwith(sprintf "%s file: has wrong namespace!" sourcefile.FullName)

// NotFollowingConsoleAppConvention
for projfile in allProjFiles do
    let isWrongConsoleApplication =
        NotFollowingConsoleAppConvention projfile true

    printfn "%A" projfile

    if isWrongConsoleApplication then
        failwith(
            sprintf
                "%s project: Should not contain console methods or printf"
                projfile.FullName
        )
