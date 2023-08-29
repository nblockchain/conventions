module FileConventions

open System
open System.IO
open System.Linq
open System.Text.RegularExpressions

open Mono
open Mono.Unix.Native
open Fsdk

let ymlAssertionError = "Bug: file should be a .yml file"
let projAssertionError = "Bug: file should be a proj file"
let sourceFileAssertionError = "Bug: file was not a F#/C# source file"

let HasCorrectShebang(fileInfo: FileInfo) =
    let fileText = File.ReadLines fileInfo.FullName

    if fileText.Any() then
        let firstLine = fileText.First()

        firstLine.StartsWith "#!/usr/bin/env fsx"
        || firstLine.StartsWith "#!/usr/bin/env -S dotnet fsi"

    else
        false

let MixedLineEndings(fileInfo: FileInfo) =
    let fileText = File.ReadAllText fileInfo.FullName

    let lf = Regex("[^\r]\n", RegexOptions.Compiled)
    let cr = Regex("\r[^\n]", RegexOptions.Compiled)
    let crlf = Regex("\r\n", RegexOptions.Compiled)

    let numberOfLineEndings =
        [
            lf.IsMatch fileText
            cr.IsMatch fileText
            crlf.IsMatch fileText
        ]
        |> Seq.filter(
            function
            | isMatch -> isMatch = true
        )
        |> Seq.length

    numberOfLineEndings > 1

let DetectUnpinnedVersionsInGitHubCI(fileInfo: FileInfo) =
    Misc.BetterAssert (fileInfo.FullName.EndsWith ".yml") ymlAssertionError

    let fileText = File.ReadAllText fileInfo.FullName

    let latestTagInRunsOnRegex =
        Regex("runs-on: .*-latest", RegexOptions.Compiled)

    latestTagInRunsOnRegex.IsMatch fileText

let DetectUnpinnedDotnetToolInstallVersions(fileInfo: FileInfo) =
    Misc.BetterAssert (fileInfo.FullName.EndsWith ".yml") ymlAssertionError

    let fileLines = File.ReadLines fileInfo.FullName

    let dotnetToolInstallRegex =
        Regex("dotnet\\s+tool\\s+install\\s+", RegexOptions.Compiled)

    let unpinnedDotnetToolInstallVersions =
        fileLines
        |> Seq.filter(fun line -> dotnetToolInstallRegex.IsMatch line)
        |> Seq.filter(fun line ->
            not(line.Contains("--version")) && not(line.Contains("-v"))
        )
        |> (fun unpinnedVersions -> Seq.length unpinnedVersions > 0)

    unpinnedDotnetToolInstallVersions

let DetectAsteriskInPackageReferenceItems(fileInfo: FileInfo) =
    Misc.BetterAssert (fileInfo.FullName.EndsWith "proj") projAssertionError

    let fileText = File.ReadAllText fileInfo.FullName

    let asteriskInPackageReference =
        Regex(
            "<PackageReference.*Version=\".*\*.*\".*/>",
            RegexOptions.Compiled
        )

    asteriskInPackageReference.IsMatch fileText

let DetectMissingVersionsInNugetPackageReferences(fileInfo: FileInfo) =
    Misc.BetterAssert
        (fileInfo.FullName.EndsWith ".fsx")
        sourceFileAssertionError

    let fileLines = File.ReadLines fileInfo.FullName

    not(
        fileLines
        |> Seq.filter(fun line -> line.StartsWith "#r \"nuget:")
        |> Seq.filter(fun line -> not(line.Contains ","))
        |> Seq.isEmpty
    )

let HasBinaryContent(fileInfo: FileInfo) =
    let lines = File.ReadLines fileInfo.FullName

    lines
    |> Seq.map(fun line ->
        line.Any(fun character ->
            Char.IsControl character && character <> '\r' && character <> '\n'
        )
    )
    |> Seq.contains true

type EolAtEof =
    | True
    | False
    | NotApplicable

let EolAtEof(fileInfo: FileInfo) =
    if HasBinaryContent fileInfo then
        NotApplicable
    else
        use streamReader = new StreamReader(fileInfo.FullName)
        let filetext = streamReader.ReadToEnd()

        if filetext <> String.Empty then
            if Seq.last filetext = '\n' then
                True
            else
                False
        else
            True

let IsFooterReference(line: string) : bool =
    line.[0] = '[' && line.IndexOf "] " > 0

let IsFixesOrClosesSentence(line: string) : bool =
    line.IndexOf "Fixes " = 0 || line.IndexOf "Closes " = 0

let IsCoAuthoredByTag(line: string) : bool =
    line.IndexOf "Co-authored-by: " = 0

let IsFooterNote(line: string) : bool =
    IsFooterReference line
    || IsCoAuthoredByTag line
    || IsFixesOrClosesSentence line

type Word =
    | CodeBlock
    | FooterNote
    | PlainText

type Text =
    {
        Type: Word
        Text: string
    }

let SplitIntoWords(text: string) =
    let codeBlockRegex = "\s*(```[\s\S]*```)\s*"

    let words =
        Regex.Split(text, codeBlockRegex)
        |> Seq.filter(fun item -> not(String.IsNullOrEmpty item))
        |> Seq.map(fun item ->
            if Regex.IsMatch(item, codeBlockRegex) then
                {
                    Text = item
                    Type = CodeBlock
                }
            else
                {
                    Text = item
                    Type = PlainText
                }
        )
        |> Seq.map(fun paragraph ->
            if paragraph.Type = CodeBlock then
                Seq.singleton paragraph
            else
                let lines = paragraph.Text.Split Environment.NewLine

                lines
                |> Seq.map(fun line ->
                    if IsFooterNote line then
                        Seq.singleton(
                            {
                                Text = line
                                Type = FooterNote
                            }
                        )
                    else
                        line.Split " "
                        |> Seq.map(fun word ->
                            {
                                Text = word
                                Type = PlainText
                            }
                        )
                )
                |> Seq.concat
        )
        |> Seq.concat

    words |> Seq.toList

let private WrapParagraph (text: string) (maxCharsPerLine: int) : string =
    let words = SplitIntoWords text

    let rec processWords
        (currentLine: string)
        (wrappedText: string)
        (remainingWords: List<Text>)
        : string =

        let isColonBreak (currentLine: string) (textAfter: Text) =
            currentLine.EndsWith ":" && Char.IsUpper textAfter.Text.[0]

        match remainingWords with
        | [] -> (wrappedText + currentLine).Trim()
        | word :: rest ->
            match currentLine, word with
            | "", _ -> processWords word.Text wrappedText rest
            | _,
              {
                  Type = PlainText
              } when
                String.length currentLine + word.Text.Length + 1
                <= maxCharsPerLine
                && not(isColonBreak currentLine word)
                ->
                processWords (currentLine + " " + word.Text) wrappedText rest
            | _,
              {
                  Type = PlainText
              } ->
                processWords
                    word.Text
                    (wrappedText + currentLine + Environment.NewLine)
                    rest
            | _, _ ->
                processWords
                    String.Empty
                    (wrappedText
                     + currentLine
                     + Environment.NewLine
                     + word.Text
                     + Environment.NewLine)
                    rest

    processWords String.Empty String.Empty words

let WrapText (text: string) (maxCharsPerLine: int) : string =
    let wrappedParagraphs =
        text.Split $"{Environment.NewLine}{Environment.NewLine}"
        |> Seq.map(fun paragraph -> WrapParagraph paragraph maxCharsPerLine)

    String.Join(
        $"{Environment.NewLine}{Environment.NewLine}",
        wrappedParagraphs
    )

let private DetectInconsistentVersions
    (fileInfos: seq<FileInfo>)
    (versionRegexPattern: string)
    =
    let versionRegex = Regex(versionRegexPattern, RegexOptions.Compiled)

    let allFilesTexts =
        fileInfos
        |> Seq.map(fun fileInfo -> File.ReadAllText fileInfo.FullName)
        |> String.concat Environment.NewLine

    let versionMap =
        versionRegex.Matches allFilesTexts
        |> Seq.fold
            (fun acc regexMatch ->
                let key = regexMatch.Groups.[1].ToString()
                let value = regexMatch.Groups.[2].ToString()

                match Map.tryFind key acc with
                | Some prevSet -> Map.add key (Set.add value prevSet) acc
                | None -> Map.add key (Set.singleton value) acc
            )
            Map.empty

    versionMap
    |> Seq.map(fun item -> Seq.length item.Value > 1)
    |> Seq.contains true

let DetectInconsistentVersionsInGitHubCIWorkflow(fileInfos: seq<FileInfo>) =
    fileInfos
    |> Seq.iter(fun fileInfo ->
        Misc.BetterAssert (fileInfo.FullName.EndsWith ".yml") ymlAssertionError
    )

    let inconsistentVersionsType1 =
        DetectInconsistentVersions
            fileInfos
            "\\swith:\\s*([^\\s]*)-version:\\s*([^\\s]*)\\s"

    let inconsistentVersionsType2 =
        DetectInconsistentVersions
            fileInfos
            "\\suses:\\s*([^\\s]*)@v([^\\s]*)\\s"

    inconsistentVersionsType1 || inconsistentVersionsType2

let DetectInconsistentVersionsInGitHubCI(dir: DirectoryInfo) =
    let ymlFiles = dir.GetFiles("*.yml", SearchOption.AllDirectories)

    if Seq.length ymlFiles = 0 then
        false
    else
        DetectInconsistentVersionsInGitHubCIWorkflow ymlFiles

let DetectInconsistentVersionsInNugetRefsInFSharpScripts
    (fileInfos: seq<FileInfo>)
    =
    fileInfos
    |> Seq.iter(fun fileInfo ->
        Misc.BetterAssert
            (fileInfo.FullName.EndsWith ".fsx")
            sourceFileAssertionError
    )

    DetectInconsistentVersions
        fileInfos
        "#r \"nuget:\\s*([^\\s]*)\\s*,\\s*Version\\s*=\\s*([^\\s]*)\\s*\""

let DetectInconsistentVersionsInFSharpScripts
    (dir: DirectoryInfo)
    (ignoreDirs: Option<seq<string>>)
    =
    let fsxFiles =
        match ignoreDirs with
        | Some ignoreDirs ->
            dir.GetFiles("*.fsx", SearchOption.AllDirectories)
            |> Seq.filter(fun fileInfo ->
                ignoreDirs
                |> Seq.filter(fun ignoreDir ->
                    fileInfo.FullName.Contains ignoreDir
                )
                |> (fun toBeIgnored -> Seq.length toBeIgnored = 0)
            )
        | None -> dir.GetFiles("*.fsx", SearchOption.AllDirectories)

    if Seq.length fsxFiles = 0 then
        false
    else
        DetectInconsistentVersionsInNugetRefsInFSharpScripts fsxFiles

let allowedNonVerboseFlags =
    seq {
        "unzip"

        // even if env in linux has --split-string=foo as equivalent to env -S, it
        // doesn't seem to be present in macOS' env man page and doesn't work either
        "env -S"
    }

let NonVerboseFlags(fileInfo: FileInfo) =
    let validExtensions =
        seq {
            ".yml"
            ".fsx"
            ".fs"
            ".sh"
        }

    let isFileExtentionValid =
        validExtensions
        |> Seq.map(fun ext -> fileInfo.FullName.EndsWith ext)
        |> Seq.contains true

    if not isFileExtentionValid then
        let sep = ","

        failwith
            $"NonVerboseFlags function only supports {String.concat sep validExtensions} files."

    let fileLines = File.ReadLines fileInfo.FullName

    let nonVerboseFlagsRegex = Regex("\\s-[a-zA-Z]\\b", RegexOptions.Compiled)

    let numInvalidFlags =
        fileLines
        |> Seq.filter(fun line ->
            let nonVerboseFlag = nonVerboseFlagsRegex.IsMatch line

            let allowedNonVerboseFlag =
                allowedNonVerboseFlags
                |> Seq.map(fun allowedFlag -> line.Contains allowedFlag)
                |> Seq.contains true

            nonVerboseFlag && not allowedNonVerboseFlag
        )
        |> Seq.length

    numInvalidFlags > 0

let IsExecutable(fileInfo: FileInfo) =
    let hasExecuteAccess = Syscall.access(fileInfo.FullName, AccessModes.X_OK)
    hasExecuteAccess = 0

let DefiningEmptyStringsWithDoubleQuotes(fileInfo: FileInfo) =
    let fileText = File.ReadAllText fileInfo.FullName
    fileText.Contains "\"\""

let ProjFilesNamingConvention(fileInfo: FileInfo) =
    let regex = Regex "(.*)\..*proj$"
    Misc.BetterAssert (regex.IsMatch fileInfo.FullName) projAssertionError
    let fileName = Path.GetFileNameWithoutExtension fileInfo.FullName

    let parentDirectoryName =
        Path.GetDirectoryName fileInfo.FullName |> Path.GetFileName

    printfn
        "File name: %s, Parent directory name: %s"
        fileName
        parentDirectoryName

    fileName <> parentDirectoryName

let DoesNamespaceInclude (fileInfo: FileInfo) (word: string) =
    let fileText = File.ReadLines fileInfo.FullName

    if fileText.Any() then
        let rightNamespace =
            fileText |> Seq.tryFind(fun x -> x.Contains "namespace")

        match rightNamespace with
        | Some fileNamespace ->
            let words =
                fileNamespace.Split(' ', StringSplitOptions.RemoveEmptyEntries)

            let namespaceCorrentPos = 1
            let namespaceWordsCount = 2

            if words.Length < namespaceWordsCount then
                false
            else
                let namespaceName = words[namespaceCorrentPos]
                namespaceName.Equals(word) || namespaceName.Equals($"{word};")
        | None ->
            // It is possible that there is a fsharp file without namespace
            if fileInfo.FullName.EndsWith ".fs"
               || fileInfo.FullName.EndsWith ".fsx" then
                true
            else
                false
    else
        false

let NotFollowingNamespaceConvention(fileInfo: FileInfo) =
    Misc.BetterAssert
        (fileInfo.FullName.EndsWith ".fs" || fileInfo.FullName.EndsWith ".cs")
        sourceFileAssertionError

    let fileName = Path.GetFileNameWithoutExtension fileInfo.FullName
    let parentDir = Path.GetDirectoryName fileInfo.FullName |> DirectoryInfo

    printfn "File name: %s, Parent directory name: %s" fileName parentDir.Name

    if parentDir.Parent.Name = "src" then
        DoesNamespaceInclude fileInfo parentDir.Name |> not

    elif parentDir.Parent.Parent.Name = "src" then
        DoesNamespaceInclude
            fileInfo
            $"{parentDir.Parent.Name}.{parentDir.Name}"
        |> not

    else
        false


let ContainsConsoleMethods(fileInfo: FileInfo) =
    let fileText = File.ReadAllText fileInfo.FullName

    let consoleMethods =
        [
            "printf"
            "Console."
            "Async.RunSynchronously"
        ]

    consoleMethods |> List.exists fileText.Contains


let NotFollowingConsoleAppConvention(fileInfo: FileInfo) =
    let fileText = File.ReadAllText fileInfo.FullName
    let parentDir = Path.GetDirectoryName fileInfo.FullName

    if not(fileText.Contains "<OutputType>Exe</OutputType>") then
        let rec allFiles dirs =
            if Seq.isEmpty dirs then
                Seq.empty
            else
                let csFiles =
                    dirs
                    |> Seq.collect(fun dir ->
                        Directory.EnumerateFiles(dir, "*.cs")
                    )

                let fsFiles =
                    dirs
                    |> Seq.collect(fun dir ->
                        Directory.EnumerateFiles(dir, "*.fs")
                    )

                let projectDirectories =
                    dirs
                    |> Seq.collect Directory.EnumerateDirectories
                    |> allFiles

                Seq.append csFiles <| Seq.append fsFiles projectDirectories

        let sourceFiles = allFiles(parentDir |> Seq.singleton)

        sourceFiles
        |> Seq.exists(fun value -> ContainsConsoleMethods(FileInfo value))

    else
        // project name should ends with .Console
        // we only check parent dir because
        // we have ProjFilesNamingConvention rule to check project name
        parentDir.EndsWith ".Console" |> not
