module fileConventions
open System.IO
open System

let HasShebang (fileInfo: FileInfo) =
    true
    // use streamReader = new StreamReader (fileInfo.FullName)
    // let fileText = streamReader.ReadToEnd()
    // fileText.[..2] = "#!"
