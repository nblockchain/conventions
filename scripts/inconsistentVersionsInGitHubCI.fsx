#!/usr/bin/env -S dotnet fsi

open System.IO

#load "../src/FileConventions/Library.fs"
#load "../src/FileConventions/Helpers.fs"

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo

let githubWorkflowsDirFromRootDir =
    Path.Combine(rootDir.FullName, ".github", "workflows") |> DirectoryInfo

let currentDir = Directory.GetCurrentDirectory() |> DirectoryInfo

let githubWorkflowsDirFromCurrentDir =
    Path.Combine(currentDir.FullName, ".github", "workflows") |> DirectoryInfo

let targetDir =
    match
        githubWorkflowsDirFromRootDir.Exists,
        githubWorkflowsDirFromCurrentDir.Exists
        with
    | true, false -> githubWorkflowsDirFromRootDir
    | false, true -> githubWorkflowsDirFromCurrentDir
    | false, false -> failwith "No .github/workflows/ subfolder found"
    | true, true ->
        Helpers.PreferLessDeeplyNestedDir
            githubWorkflowsDirFromCurrentDir
            githubWorkflowsDirFromRootDir

let inconsistentVersionsInGitHubCI =
    FileConventions.DetectInconsistentVersionsInGitHubCI targetDir

if inconsistentVersionsInGitHubCI then
    failwith
        "You shouldn't use inconsistent versions in `uses: foo@bar` or `with: foo-version: bar` statements in GitHubCI files."
