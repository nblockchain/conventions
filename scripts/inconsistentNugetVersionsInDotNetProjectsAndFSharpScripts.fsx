#!/usr/bin/env -S dotnet fsi

open System.IO

#r "System.Core.dll"
#r "System.Xml.Linq.dll"

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"
#r "nuget: Microsoft.Build, Version=16.11.0"
#r "nuget: Mono.Unix, Version=7.1.0-final.1.21458.1"
#r "nuget: YamlDotNet, Version=16.1.3"

#load "../src/FileConventions/Library.fs"
#load "../src/FileConventions/Helpers.fs"
#load "../src/FileConventions/NugetVersionsCheck.fs"
#load "../src/FileConventions/CombinedVersionCheck.fs"

let currentDir = Directory.GetCurrentDirectory()

let targetDir = currentDir |> DirectoryInfo

let versionsMap =
    CombinedVersionCheck.GetVersionsMapForNugetRefsInFSharpScriptsAndProjects
        (Helpers.GetFiles targetDir "*.fsx")
        (NugetVersionsCheck.FindProjectFiles(
            NugetVersionsCheck.Folder targetDir
        ))

let packagesWithMultipleVersions =
    versionsMap |> Map.filter(fun _ versions -> versions.Count > 1)

if not packagesWithMultipleVersions.IsEmpty then
    for KeyValue(packageName, versions) in packagesWithMultipleVersions do
        let versionsString =
            System.String.Join(
                ", ",
                versions |> Seq.map(fun version -> $"\"{version}\"")
            )

        System.Console.Error.WriteLine
            $"Found {versions.Count} different versions of package \"{packageName}\": {versionsString}"

    System.Environment.Exit 1
