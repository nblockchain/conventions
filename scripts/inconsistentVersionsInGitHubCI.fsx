#!/usr/bin/env -S dotnet fsi

open System.IO

#load "../src/FileConventions/Library.fs"

let githubWorkflowsDir =
    Path.Combine(__SOURCE_DIRECTORY__, "../.github/workflows") |> DirectoryInfo

let inconsistentVersionsInGitHubCI =
    FileConventions.DetectInconsistentVersionsInGitHubCI githubWorkflowsDir

if inconsistentVersionsInGitHubCI then
    failwith
        "You shouldn't use inconsistent versions in `uses: foo@bar` or `with: foo-version: bar` statements in GitHubCI files."
