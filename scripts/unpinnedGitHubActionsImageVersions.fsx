#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#load "../src/FileConventions/Library.fs"
#load "../src/FileConventions/Helpers.fs"

let invalidFiles =
    Helpers.GetInvalidFiles
        "."
        "*.yml"
        FileConventions.DetectUnpinnedVersionsInGitHubCI

let message =
    "The following files shouldn't contain `-latest` in `runs-on:` GitHubCI tags."
    + Environment.NewLine
    + "Here is a list of available runner image versions that you can use:"
    + Environment.NewLine
    + "https://github.com/actions/runner-images#available-images"

Helpers.AssertNoInvalidFiles invalidFiles message
