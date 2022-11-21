module FileConventions

open System
open System.IO
open System.Linq

let HasBinaryContent (fileInfo: FileInfo) =
    let lines = File.ReadLines fileInfo.FullName 
    lines 
        |> Seq.map (fun line -> line.Any(fun character -> Char.IsControl(character) && character <> '\r' && character <> '\n'))
        |> Seq.contains true 
