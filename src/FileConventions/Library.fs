module FileConventions

open System
open System.IO
open System.Linq

let HasBinaryContent (fileInfo: FileInfo) =
    use streamReader = new StreamReader (fileInfo.FullName)
    let content = streamReader.ReadToEnd()
    content.Any(fun ch -> Char.IsControl(ch) && ch <> '\r' && ch <> '\n')
