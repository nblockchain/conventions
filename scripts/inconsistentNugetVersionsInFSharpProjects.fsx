#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "System.Core.dll"
#r "System.Xml.Linq.dll"

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"

open Fsdk

#r "nuget: Microsoft.Build, Version=16.11.0"

#load "../src/FileConventions/NugetVersionsCheck.fs"

open NugetVersionsCheck

let args = Misc.FsxOnlyArguments()
let currentDirectory = Directory.GetCurrentDirectory()

if args.Length > 1 then
    printf "Usage: %s [example.sln(optional)]" __SOURCE_FILE__

    Environment.Exit 1

let target =
    if args.IsEmpty then
        currentDirectory |> DirectoryInfo |> ScriptTarget.Folder
    else
        let singleArg = args.[0]

        if Directory.Exists singleArg then
            failwithf "Use an .sln file insted of directory."
        elif not(File.Exists singleArg) then
            failwithf "'%s' does not exist." singleArg
        elif singleArg.EndsWith ".sln" then
            singleArg |> FileInfo |> ScriptTarget.Solution
        else
            failwithf
                "'%s' argument is invalid. You should enter an .sln file or run this script on current directory"
                singleArg


let nugetSolutionPackagesDir =
    Path.Combine(currentDirectory, "packages") |> DirectoryInfo

let nugetPackageConfigDir =
    Path.Combine(currentDirectory, "NuGet.config") |> FileInfo

if not nugetPackageConfigDir.Exists || not nugetSolutionPackagesDir.Exists then
    failwithf
        "NuGet.config not found in '%s', please create it with the `globalPackagesFolder` key as `packages`."
        nugetPackageConfigDir.FullName

SanityCheckNugetPackages target currentDirectory nugetSolutionPackagesDir
