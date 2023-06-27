#!/usr/bin/env -S dotnet fsi

open System.IO

#load "../src/FileConventions/Library.fs"
#load "../src/FileConventions/Helpers.fs"

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo

let inconsistentVersionsInFsharpScripts =
    Helpers.GetFiles rootDir "*.fsx"
    |> FileConventions.DetectInconsistentVersionsInNugetRefsInFSharpScripts

if inconsistentVersionsInFsharpScripts then
    failwith
        "You shouldn't use inconsistent versions in nuget package references of F# scripts."
