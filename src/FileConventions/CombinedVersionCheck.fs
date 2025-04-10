module CombinedVersionCheck

open System.IO

let GetVersionsMapForNugetRefsInFSharpScriptsAndProjects
    (fsxFileInfos: #seq<FileInfo>)
    (fsprojFileInfos: #seq<FileInfo>)
    : Map<string, Set<string>> =
    let versionsInFsharpScripts =
        FileConventions.GetVersionsMapForNugetRefsInFSharpScripts fsxFileInfos

    let versionsInProjects = NugetVersionsCheck.GetVersionsMap fsprojFileInfos

    NugetVersionsCheck.MapHelper.MergeMaps
        versionsInFsharpScripts
        versionsInProjects

let DetectInconsistentVersionsInNugetRefsInFSharpScripts
    (fsxFileInfos: #seq<FileInfo>)
    (fsprojFileInfos: #seq<FileInfo>)
    : bool =
    GetVersionsMapForNugetRefsInFSharpScriptsAndProjects
        fsxFileInfos
        fsprojFileInfos
    |> Map.exists(fun _ versions -> versions.Count > 1)
