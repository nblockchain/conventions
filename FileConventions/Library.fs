module FileConventions

open System.Linq
open System.IO
open System

let HasShebang (fileInfo: FileInfo) =
    let fileText = File.ReadLines(fileInfo.FullName).First()
    fileText.StartsWith("#!")

