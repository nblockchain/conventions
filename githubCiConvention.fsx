open System
open System.IO
open System.Text.RegularExpressions

let IsInDir (dirName: string) (fileInfo: FileInfo) =
    (fileInfo.FullName.Contains $"%c{Path.DirectorySeparatorChar}%s{dirName}%c{Path.DirectorySeparatorChar}")

let InvalidRunnerEnvironment(fileInfo: FileInfo) =
    let regex = Regex("runs-on:.*-latest", RegexOptions.Compiled)
    let streamReader = new StreamReader(fileInfo.FullName)
    let fileStr = streamReader.ReadToEnd()
    regex.IsMatch(fileStr)

let invalidGithubCiFiles = 
    Directory.GetFiles(".", "*.*", SearchOption.AllDirectories) 
    |> Seq.map (fun pathStr -> FileInfo pathStr)
    |> Seq.filter (IsInDir $".github%c{Path.DirectorySeparatorChar}workflows")
    |> Seq.filter (InvalidRunnerEnvironment)

if Seq.length invalidGithubCiFiles > 0 then
    let message = 
        "The github CI runner environment should use a specific version rather than latest version in the following files:" + 
        Environment.NewLine + 
        (invalidGithubCiFiles 
        |> Seq.map (fun fileInfo -> fileInfo.FullName)
        |> String.concat Environment.NewLine)
        
    failwith message
