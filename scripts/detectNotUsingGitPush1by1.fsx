#!/usr/bin/env -S dotnet fsi

open System
open System.Net.Http
open System.Net.Http.Headers

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"

let gitRepo = Environment.GetEnvironmentVariable "GITHUB_REPOSITORY"

if gitRepo = null then
    failwith "You shouldn't use this script outside CI."

let currentBranch =
    Fsdk
        .Process
        .Execute(
            {
                Command = "git"
                Arguments = "rev-parse --abbrev-ref HEAD"
            },
            Fsdk.Process.Echo.Off
        )
        .UnwrapDefault()
        .Trim()

let prCommits =
    Fsdk
        .Process
        .Execute(
            {
                Command = "git"
                Arguments =
                    sprintf "rev-list %s~..%s" currentBranch currentBranch
            },
            Fsdk.Process.Echo.Off
        )
        .UnwrapDefault()
        .Trim()
        .Split "\n"
    |> Seq.tail

let notUsingGitPush1by1 =
    prCommits
    |> Seq.map(fun commit ->
        use client: HttpClient = new HttpClient()
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

        let json = client.GetStringAsync(url).Result

        json.Contains "\"check_suites\":[]"
    )
    |> Seq.contains true

if notUsingGitPush1by1 then
    failwith(
        "Please push the commits one by one.\n"
        + "You may use this script:\n"
        + "https://github.com/nblockchain/fsx/blob/master/Tools/gitPush1by1.fsx"
    )
