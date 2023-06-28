#!/usr/bin/env -S dotnet fsi

open System.IO
open System.Linq

#load "../src/FileConventions/Library.fs"
#load "../src/FileConventions/Helpers.fs"

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo
let currentDir = Directory.GetCurrentDirectory() |> DirectoryInfo

let targetDir = Helpers.PreferLessDeeplyNestedDir currentDir rootDir

let inconsistentVersionsInFsharpScripts =
    Helpers.GetFiles targetDir "*.fsx"
    |> FileConventions.DetectInconsistentVersionsInNugetRefsInFSharpScripts

if inconsistentVersionsInFsharpScripts then
    failwith
        "You shouldn't use inconsistent versions in nuget package references of F# scripts."
