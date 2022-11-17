open System.IO
open System

let EolAtEof(fileInfo: FileInfo) = 
    use streamReader = new StreamReader (fileInfo.FullName)
    let filetext = streamReader.ReadToEnd()
    
    if filetext <> String.Empty then
        Seq.last filetext = '\n'
    else
        true

let NotInDir (dirName: string) (fileInfo: FileInfo) =
    not (fileInfo.FullName.Contains $"%c{Path.DirectorySeparatorChar}%s{dirName}%c{Path.DirectorySeparatorChar}")

let whitelist = [".svg"; ".png"; ".slnf"]

let invalidFiles = 
    Directory.GetFiles(".", "*.*", SearchOption.AllDirectories) 
    |> Seq.map (fun pathStr -> FileInfo pathStr)
    |> Seq.filter (NotInDir "node_modules")
    |> Seq.filter (NotInDir ".git")
    |> Seq.filter (fun fileInfo -> not (EolAtEof fileInfo))
    |> Seq.filter (fun fileInfo -> not (List.contains fileInfo.Extension whitelist))

if Seq.length invalidFiles > 0 then
    let message = 
        "The following files don't end with EOL:" + 
        Environment.NewLine + 
        (invalidFiles 
        |> Seq.map (fun fileInfo -> fileInfo.FullName)
        |> String.concat Environment.NewLine)
        
    failwith message
