open System.IO

let EolAtEof(path: FileInfo) = 

    use streamReader = new StreamReader (path.FullName)
    let filetext = streamReader.ReadToEnd()
    
    if filetext <> "" then
        (filetext |> Seq.last = '\n')
    else
        true

let NotPrefix (prefix: string) (mainStr: string) =
    not (mainStr.StartsWith(prefix))

let invalidFiles = 
    Directory.GetFiles(".", "*.*", SearchOption.AllDirectories) 
    |> Seq.filter (NotPrefix "./node_modules")
    |> Seq.filter (NotPrefix "./.git")
    |> Seq.map (fun pathStr -> FileInfo pathStr)
    |> Seq.filter (fun fileInfo -> not (EolAtEof fileInfo))

if invalidFiles |> Seq.length > 0 then
    let mutable message = "The following files doesn't end with EOL:\n" 
    invalidFiles |> Seq.iter (fun x -> message <- message + x.FullName + "\n")
    failwith message
