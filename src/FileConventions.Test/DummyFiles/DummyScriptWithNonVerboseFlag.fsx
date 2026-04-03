#!/usr/bin/env -S dotnet fsi

#r "nuget: Fsdk, Version=0.6.1--date20260403-0728.git-c9a0eae"

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
