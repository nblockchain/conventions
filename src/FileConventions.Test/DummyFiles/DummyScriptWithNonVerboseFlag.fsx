#!/usr/bin/env -S dotnet fsi

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"

open Fsdk
open Fsdk.Process

let gitRemote =
    {
        Command = "git"
        Arguments = "remote -v"
    }

let gitRemoteOutput =
    Process
        .Execute(gitRemote, Echo.All)
        .UnwrapDefault()
