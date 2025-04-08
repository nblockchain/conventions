module CombinedVersionCheck

open System.IO

let DetectInconsistentVersionsInNugetRefsInFSharpScripts
    (_fsxFileInfos: #seq<FileInfo>)
    (_fsprojFileInfos: #seq<FileInfo>)
    : bool =
    raise <| System.NotImplementedException()
