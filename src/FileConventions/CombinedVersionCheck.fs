module CombinedVersionCheck

open System.IO

let GetVersionsMapForNugetRefsInFSharpScriptsAndProjects
    (fsxFileInfos: #seq<FileInfo>)
    (projectFileInfos: #seq<FileInfo>)
    : Map<string, Set<string>> =
    let versionsInFsharpScripts =
        FileConventions.GetVersionsMapForNugetRefsInFSharpScripts fsxFileInfos

    let versionsInProjects = NugetVersionsCheck.GetVersionsMap projectFileInfos

    NugetVersionsCheck.MapHelper.MergeMaps
        versionsInFsharpScripts
        versionsInProjects

let DetectInconsistentVersionsInNugetRefsInFSharpScripts
    (fsxFileInfos: #seq<FileInfo>)
    (projectFileInfos: #seq<FileInfo>)
    : bool =
    GetVersionsMapForNugetRefsInFSharpScriptsAndProjects
        fsxFileInfos
        projectFileInfos
    |> Map.exists(fun _ versions -> versions.Count > 1)
