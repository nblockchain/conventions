open System.IO
open System

let MixedLineEnding(fileInfo: FileInfo) = 
    use streamReader = new StreamReader (fileInfo.FullName)
    let fileText = streamReader.ReadToEnd()

    let numberOfLineEndings = ([| "\n"; "\r\n"; "\r" |] 
        |> Seq.filter(function ending -> fileText.IndexOf(ending) > 0)
        |> Seq.length)

    numberOfLineEndings > 1

let NotInDir (dirName: string) (fileInfo: FileInfo) =
    not (fileInfo.FullName.Contains $"%c{Path.DirectorySeparatorChar}%s{dirName}%c{Path.DirectorySeparatorChar}")

let invalidFiles = 
    Directory.GetFiles(".", "*.*", SearchOption.AllDirectories) 
    |> Seq.map (fun pathStr -> FileInfo pathStr)
    |> Seq.filter (NotInDir "node_modules")
    |> Seq.filter (NotInDir ".git")
    |> Seq.filter MixedLineEnding

if Seq.length invalidFiles > 0 then
    let message = 
        "The following files shouldn't use mixed line endings:" + 
        Environment.NewLine + 
        (invalidFiles 
        |> Seq.map (fun fileInfo -> fileInfo.FullName)
        |> String.concat Environment.NewLine)
        
    failwith message
