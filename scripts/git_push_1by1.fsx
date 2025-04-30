#!/usr/bin/env -S dotnet fsi

open System
open System.IO
open System.Linq
open System.Threading

#r "System.Configuration"
open System.Configuration

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"

open Fsdk
open Fsdk.Process

// mimic https://stackoverflow.com/a/3230241/544947
let GitSpecificPush
    (remoteName: string)
    (commitSha: string)
    (remoteBranchName: string)
    (force: bool)
    =
    let forceFlag =
        if force then
            "--force"
        else
            "--force-with-lease"

    let gitPush =
        {
            Command = "git"
            Arguments =
                sprintf
                    "push %s %s:refs/heads/%s %s"
                    remoteName
                    commitSha
                    remoteBranchName
                    forceFlag
        }

    let pushProc = Process.Execute(gitPush, Echo.OutputOnly)

    match pushProc.Result with
    | Error _ -> failwith "Push failed ^"
    | WarningsOrAmbiguous _
    | Success _ -> ()

let GitFetch(remoteOpt: Option<string>) =
    let remoteArg =
        match remoteOpt with
        | None -> "--all"
        | Some remote -> remote

    let gitFetch =
        {
            Command = "git"
            Arguments = sprintf "fetch %s" remoteArg
        }

    let fetchProc = Process.Execute(gitFetch, Echo.OutputOnly)

    match fetchProc.Result with
    | Error _ -> failwith "Fetch failed ^"
    | WarningsOrAmbiguous _
    | Success _ -> ()

let GetLastNthCommitFromRemoteBranch
    (remoteName: string)
    (remoteBranch: string)
    (n: uint32)
    =
    let gitShow =
        {
            Command = "git"
            Arguments =
                sprintf "show %s/%s~%i --no-patch" remoteName remoteBranch n
        }

    let gitShowProcOutput = Process.Execute(gitShow, Echo.Off).UnwrapDefault()

    let firstLine =
        (Misc.CrossPlatformStringSplitInLines gitShowProcOutput)
            .First()

    // split this line: commit 938634a3e7d4dc7e6dd357927a16165120bbea68 (HEAD -> master, origin/master, origin/HEAD)
    let commitHash = firstLine.Split([| " " |], StringSplitOptions.None).[1]
    commitHash

let FindUnpushedCommits (remoteName: string) (remoteBranch: string) =
    let rec findUnpushedCommits
        localCommitsWalkedSoFar
        currentSkipCount
        remoteCommits
        =
        let rec findIntersection localCommits (remoteCommits: List<string>) =
            match localCommits with
            | [] -> None
            | head :: tail ->
                if remoteCommits.Contains head then
                    Some tail
                else
                    findIntersection tail remoteCommits

        Console.WriteLine "Walking tree..."

        let currentHash =
            Process
                .Execute(
                    {
                        Command = "git"
                        Arguments =
                            sprintf
                                "log -1 --skip=%i --format=format:%%H"
                                currentSkipCount
                    },
                    Echo.Off
                )
                .UnwrapDefault()
                .Trim()

        let newRemoteCommits =
            (GetLastNthCommitFromRemoteBranch
                remoteName
                remoteBranch
                currentSkipCount)
            :: remoteCommits

        let newLocalCommitsWalkedSoFar = currentHash :: localCommitsWalkedSoFar

        match findIntersection newLocalCommitsWalkedSoFar newRemoteCommits with
        | Some theCommitsToPush -> theCommitsToPush
        | None ->
            findUnpushedCommits
                newLocalCommitsWalkedSoFar
                (currentSkipCount + 1u)
                newRemoteCommits

    GitFetch(Some remoteName)
    findUnpushedCommits List.empty 0u List.empty

let GetLastCommits(count: UInt32) =
    let rec getLastCommits commitsFoundSoFar currentSkipCount currentCount =
        if currentCount = 0u then
            commitsFoundSoFar
        else
            let currentHash =
                Process
                    .Execute(
                        {
                            Command = "git"
                            Arguments =
                                sprintf
                                    "log -1 --skip=%i --format=format:%%H"
                                    currentSkipCount
                        },
                        Echo.Off
                    )
                    .UnwrapDefault()
                    .Trim()

            getLastCommits
                (currentHash :: commitsFoundSoFar)
                (currentSkipCount + 1u)
                (currentCount - 1u)

    getLastCommits List.empty 0u count

let remotes = Git.GetRemotes()

if not(remotes.Any()) then
    Console.Error.WriteLine "No remotes found, please add one first."
    Environment.Exit 5

let args = Misc.FsxOnlyArguments()

if args.Length > 3 then
    Console.Error.WriteLine
        "Usage: gitpush.fsx [remoteName(optional)] [numberOfCommits(optional)]"

    Environment.Exit 1

let maybeRemote, maybeNumberOfCommits, force =
    if args.Length > 1 then
        match UInt32.TryParse args.[1] with
        | true, 0u ->
            Console.Error.WriteLine
                "Second argument should be an integer higher than zero"

            Environment.Exit 2
            failwith "Unreachable"
        | true, num ->
            let numberOfCommits = Some num
            let remote = Some args.[0]

            let force =
                if args.Length = 3 then
                    if args.[2] = "-f" || args.[2] = "--force" then
                        true
                    else
                        false
                else
                    false

            remote, numberOfCommits, force
        | _ ->
            Console.Error.WriteLine "Second argument should be an integer"
            Environment.Exit 3
            failwith "Unreachable"
    elif args.Length = 0 then
        None, None, false
    else // if args.Length = 1 then
        match UInt32.TryParse args.[0] with
        | true, 0u ->
            Console.Error.WriteLine
                "Argument for the number of commits should be an integer higher than zero"

            Environment.Exit 2
            failwith "Unreachable"
        | true, num ->
            let numberOfCommits = Some num
            let remote = None
            remote, numberOfCommits, false
        | _ ->
            let numberOfCommits = None
            let remote = Some(args.[0])
            remote, numberOfCommits, false

let remote, remoteUrl =
    match maybeRemote with
    | Some remoteProvided ->
        match
            Seq.tryFind
                (fun (currentRemote, _) -> currentRemote = remoteProvided)
                remotes
            with
        | None ->
            Console.Error.WriteLine(
                sprintf "Remote '%s' not found" remoteProvided
            )

            Environment.Exit 4
            failwith "unreachable"
        | Some remote -> remote
    | None ->
        if remotes.Count() > 1 then
            Console.Error.WriteLine
                "Usage: gitpush.fsx <remoteName> [numberOfCommits(optional)]"

            Environment.Exit 6

        remotes.ElementAt 0

let currentBranch = Git.GetCurrentBranch()

let commitsToBePushed =
    match maybeNumberOfCommits with
    | None ->
        let commitsToPush = FindUnpushedCommits remote currentBranch

        if commitsToPush.Length = 0 then
            Console.Error.WriteLine(
                sprintf
                    "Current branch '%s' in remote '%s' is already up to date. Force push by specifying number of commits as 2nd argument?"
                    currentBranch
                    remote
            )

            Environment.Exit 5
            failwith "Unreachable"
        elif commitsToPush.Length = 1 then
            // no need to ask for confirmation since 1 commit doesn't need to be separated from other commits
            // (one by one doesn't apply to a length of one)
            commitsToPush
        else //if commitsToPush.Length > 1 then
            Console.WriteLine(
                sprintf
                    "Detected a delta of %i commits between local branch '%s' and the one in remote '%s', to be pushed one by one. Press any key to continue or CTRL+C to abort."
                    commitsToPush.Length
                    currentBranch
                    remote
            )

            Console.ReadKey true |> ignore
            Console.WriteLine "Pushing..."
            commitsToPush
    | Some numberOfCommits -> GetLastCommits numberOfCommits

let numberOfCommitsToPush = commitsToBePushed.Length

for commit in commitsToBePushed do
    GitSpecificPush remote commit currentBranch force

if numberOfCommitsToPush > 1 && remoteUrl.Contains "gitlab" then
    Console.WriteLine
        "NOTE: if you have issues with pipelines being canceled, visit https://gitlab.com/yourUserNameOrOrgName/yourRepoName/-/settings/ci_cd"

    Console.WriteLine
        "then click 'Expand' on 'General Pipelines', and uncheck 'Auto-cancel redundant pipelines'"
