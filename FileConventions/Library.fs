module FileConventions
open System.IO
open System

let HasShebang (fileInfo: FileInfo) =
    let fileText = File.ReadAllText(fileInfo.FullName)
    fileText.StartsWith("#!")

