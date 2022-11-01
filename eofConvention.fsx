open System.IO
open System

let EolAtEof(fileInfo: FileInfo) = 
    use streamReader = new StreamReader (fileInfo.FullName)
    let filetext = streamReader.ReadToEnd()
    
    if filetext <> String.Empty then
        (filetext |> Seq.last |> Char.ToString = Environment.NewLine)
    else
        true

let NotPrefix (prefix: FileInfo) (pathStr: FileInfo) =
    not (pathStr.FullName.StartsWith prefix.FullName)

let invalidFiles = 
    Directory.GetFiles(".", "*.*", SearchOption.AllDirectories) 
    |> Seq.map (fun pathStr -> FileInfo pathStr)
    |> Seq.filter (NotPrefix (FileInfo "./node_modules"))
    |> Seq.filter (NotPrefix (FileInfo "./.git"))
    |> Seq.filter (fun fileInfo -> not (EolAtEof fileInfo))

if Seq.length invalidFiles > 0 then
    let message = 
        "The following files don't end with EOL:" + 
        Environment.NewLine + 
        (invalidFiles 
        |> Seq.map (fun fileInfo -> fileInfo.FullName)
        |> String.concat Environment.NewLine)
        
    failwith message
