#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"
#load "../src/FileConventions/Helpers.fs"

let checkSuites =
    Fsdk
        .Process
        .Execute(
            {
                Command = "curl"
                Arguments = "https://api.github.com/repos/realmarv/conventions/commits/12b9e47a72be2e1ee87df118805780a2f29ff312/check-suites"
            },
            Fsdk.Process.Echo.All
        )

printfn "%A" checkSuites
