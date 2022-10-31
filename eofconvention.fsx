open System.IO

let EOFIsEOL(path: FileInfo) = 
    try
        use sr = new StreamReader (path.FullName)
        let filetext = sr.ReadToEnd()
        (filetext |> Seq.last = '\n')
        
    with
        | :? System.ArgumentException -> printfn "%A" path; true
        

let EOFIsNotEOL(path: FileInfo) = not (EOFIsEOL path)

let NotPrefix (value: string) (str: string) =
    not (str.StartsWith(value))


let invalidFiles = 
    Directory.GetFiles(".", "*.*", SearchOption.AllDirectories) 
    |> Array.filter (NotPrefix "./node_modules")
    |> Array.filter (NotPrefix "./.git")
    |> Array.map (fun x -> FileInfo(x))
    |> Array.filter EOFIsNotEOL

if invalidFiles.Length > 0 then
    let mutable message = "The following files doesn't end with EOL:\n" 
    invalidFiles |> Array.iter (fun x -> message <- message + x.FullName + "\n")
    raise (System.Exception(message))
