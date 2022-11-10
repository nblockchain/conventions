module fileConventions
open System.IO
open System

let HasShebang (fileInfo: FileInfo) =
    use streamReader = new StreamReader (fileInfo.FullName)
    let fileText = streamReader.ReadToEnd()
    fileText.StartsWith("#!")

