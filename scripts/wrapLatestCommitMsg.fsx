#!/usr/bin/env -S dotnet fsi

open System.IO
open System
open System.Text.RegularExpressions
open System.Linq

#r "nuget: Mono.Unix, Version=7.1.0-final.1.21458.1"
#r "nuget: YamlDotNet, Version=16.1.3"

#load "../src/FileConventions/Config.fs"
#load "../src/FileConventions/Helpers.fs"
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
    let singleEolToJustSeparateLines = 1u

    let lines =
        FileConventions.SplitByEOLs commitMsg singleEolToJustSeparateLines

    if lines.Length = 1 then
        commitMsg, None
    else
        let body = String.Join(Environment.NewLine, lines.Skip 2)
        lines.[0], Some body

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
