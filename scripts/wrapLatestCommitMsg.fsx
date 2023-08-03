#!/usr/bin/env -S dotnet fsi

open System.IO
open System
open System.Text.RegularExpressions

#load "../src/FileConventions/Library.fs"

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"

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
    // We only want to split the msg into two parts (header and body)
    let acceptablePartCount = 2
    let separatorEolCount = 1

    let msgParts =
        FileConventions.SplitByEOLs
            commitMsg
            separatorEolCount
            StringSplitOptions.RemoveEmptyEntries
            (Some acceptablePartCount)

    if msgParts.Length > 1 then
        msgParts.[0].Trim(), Some(msgParts.[1].Trim())
    else
        msgParts.[0].Trim(), None

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
        header
        + Environment.NewLine
        + Environment.NewLine
        + wrappedBody
        + Environment.NewLine
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
