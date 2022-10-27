open System.IO

let EOFIsEOL(path: string) = 
    use sr = new StreamReader (path)
    let filetext = sr.ReadToEnd()
    let eofIsEol = (filetext |> Seq.last = '\n')
    eofIsEol

let EOFIsNotEOL(path: string) = not (EOFIsEOL path)

printfn "%A" (Directory.GetFiles("./") |> Array.filter EOFIsNotEOL)