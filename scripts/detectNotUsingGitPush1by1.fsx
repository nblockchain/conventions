#!/usr/bin/env -S dotnet fsi

open System
open System.Net.Http
open System.Net.Http.Headers

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"
open Fsdk


let gitRepo = Environment.GetEnvironmentVariable "GITHUB_REPOSITORY"

if String.IsNullOrEmpty gitRepo then
    Console.Error.WriteLine
        "This script is meant to be used only within a GitHubCI pipeline"

    Environment.Exit 2

let currentBranch =
    Process
        .Execute(
            {
                Command = "git"
                Arguments = "rev-parse --abbrev-ref HEAD"
            },
            Process.Echo.Off
        )
        .UnwrapDefault()
        .Trim()

let prCommits =
    Process
        .Execute(
            {
                Command = "git"
                Arguments =
                    sprintf "rev-list %s~..%s" currentBranch currentBranch
            },
            Process.Echo.Off
        )
        .UnwrapDefault()
        .Trim()
        .Split "\n"
    |> Seq.tail

let notUsingGitPush1by1 =
    prCommits
    |> Seq.map(fun commit ->
        use client = new HttpClient()
        client.DefaultRequestHeaders.Accept.Clear()

        client.DefaultRequestHeaders.Accept.Add(
            MediaTypeWithQualityHeaderValue "application/vnd.github+json"
        )

        client.DefaultRequestHeaders.Add("User-Agent", ".NET App")
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28")

        let url =
            sprintf
                "https://api.github.com/repos/%s/commits/%s/check-suites"
                gitRepo
                commit

        let json = (client.GetStringAsync url).Result

        json.Contains "\"check_suites\":[]"
    )
    |> Seq.contains true

if notUsingGitPush1by1 then
    let errMsg =
        sprintf
            "Please push the commits one by one; using this script is recommended:%s%s"
            Environment.NewLine
            "https://github.com/nblockchain/fsx/blob/master/Tools/gitPush1by1.fsx"

    Console.Error.WriteLine errMsg
    Environment.Exit 1
