open System.IO
open System

#load "../src/FileConventions/Library.fs"

let EolAtEof(fileInfo: FileInfo) = 
    use streamReader = new StreamReader (fileInfo.FullName)
    let filetext = streamReader.ReadToEnd()
    
    if filetext <> String.Empty then
        Seq.last filetext = '\n'
    else
        true

let InDir (dirName: string) (fileInfo: FileInfo) = 
    fileInfo.FullName.Contains $"%c{Path.DirectorySeparatorChar}%s{dirName}%c{Path.DirectorySeparatorChar}"

let NotInDirs (dirNames: List<string>) (fileInfo: FileInfo) =
    not (dirNames 
        |> Seq.map (fun dirName -> InDir dirName fileInfo)
        |> Seq.contains true)

let whitelistFolders = ["node_modules"; ".git"; "Debug"; "obj"; "bin"; "DummyFiles"]

let invalidFiles = 
    Directory.GetFiles(".", "*.*", SearchOption.AllDirectories) 
    |> Seq.map (fun pathStr -> FileInfo pathStr)
    |> Seq.filter (NotInDirs whitelistFolders)
    |> Seq.filter (fun fileInfo -> not (FileConventions.HasBinaryContent fileInfo))
    |> Seq.filter (fun fileInfo -> not (EolAtEof fileInfo))

if Seq.length invalidFiles > 0 then
    let message = 
        "The following files don't end with EOL:" + 
        Environment.NewLine + 
        (invalidFiles 
        |> Seq.map (fun fileInfo -> fileInfo.FullName)
        |> String.concat Environment.NewLine)
        
    failwith message
