#!/usr/bin/env -S dotnet fsi

open System.IO
open System
open System.Text.RegularExpressions

#r "nuget: Mono.Unix, Version=7.1.0-final.1.21458.1"
#r "nuget: Fsdk, Version=0.6.0--date20230821-0702.git-5488853"

#load "../src/FileConventions/Helpers.fs"
#load "../src/FileConventions/Library.fs"

open Fsdk
open Fsdk.Process

let commitMsg =
    Fsdk
        .Process
        .Execute(
            {
                Command = "git"
                Arguments = "log -1 --format=%B"
            },
            Echo.Off
        )
        .UnwrapDefault()
        .Trim()

let header, maybeBody =
    let newLineIndex = commitMsg.IndexOf Environment.NewLine

    if newLineIndex > 0 then
        commitMsg.Substring(0, newLineIndex).Trim(),
        Some(commitMsg.Substring(newLineIndex).Trim())
    else
        commitMsg, None

let maxCharsPerLine = 64

let maybeWrappedBody =
    match maybeBody with
    | Some body -> Some(FileConventions.WrapText body maxCharsPerLine)
    | _ -> None

let EscapeDoubleQuotes(text: string) =
    Regex.Replace(text, @"([^\\])""", @"$1\""")

let newCommitMsg =
    match maybeWrappedBody with
    | Some wrappedBody ->
        header + Environment.NewLine + Environment.NewLine + wrappedBody
    | _ -> header

Fsdk
    .Process
    .Execute(
        {
            Command = "git"
            Arguments =
                $"commit --amend --message \"{EscapeDoubleQuotes newCommitMsg}\""
        },
        Echo.Off
    )
    .UnwrapDefault()
    .Trim()
