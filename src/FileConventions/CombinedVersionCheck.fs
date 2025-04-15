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

let GetFsxAndProjectFilesForDirectories(directories: #seq<DirectoryInfo>) =
    directories
    |> Seq.fold
        (fun (fsxFiles, projectFiles) dir ->
            (Seq.append fsxFiles (Helpers.GetFiles dir "*.fsx")),
            (Seq.append
                projectFiles
                (NugetVersionsCheck.FindProjectFiles(
                    NugetVersionsCheck.Folder dir
                )))
        )
        (Seq.empty, Seq.empty)
