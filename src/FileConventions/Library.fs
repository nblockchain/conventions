module FileConventions

open System
open System.IO
open System.Linq

let HasCorrectShebang (fileInfo: FileInfo) =
    let fileText = File.ReadLines fileInfo.FullName
    if fileText.Any() then
        let firstLine = fileText.First()
        
        firstLine.StartsWith "#!/usr/bin/env fsx" || 
        firstLine.StartsWith "#!/usr/bin/env -S dotnet fsi"
        
    else
        false

let MixedLineEndings(fileInfo: FileInfo) =
    use streamReader = new StreamReader (fileInfo.FullName)
    let fileText = streamReader.ReadToEnd()

    let numberOfLineEndings = ([| "\n"; "\r\n"; "\r" |]
        |> Seq.filter(function ending -> fileText.IndexOf(ending) > 0)
        |> Seq.length)

    numberOfLineEndings > 1
