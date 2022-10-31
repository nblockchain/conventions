open System.IO

let EOFIsEOL(path: string) = 
    try
        use sr = new StreamReader (path)
        let filetext = sr.ReadToEnd()
        (filetext |> Seq.last = '\n')
        
    with ex ->
        // printfn "%A" ex
        true
    
let EOFIsNotEOL(path: string) = not (EOFIsEOL path)

printfn "%A" (
    Directory.GetFiles("./", "*.*", SearchOption.AllDirectories) 
    |> Array.filter EOFIsNotEOL
    )