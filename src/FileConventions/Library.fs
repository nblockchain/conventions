module FileConventions

open System
open System.IO
open System.Linq
open System.Text.RegularExpressions

open Mono
open Mono.Unix.Native
open YamlDotNet.RepresentationModel

let SplitByEOLs (text: string) (numberOfEOLs: uint) =
    if numberOfEOLs = 0u then
        invalidArg
            (nameof numberOfEOLs)
            "Should be higher than zero to be able to split anything"

    let unixEol = "\n"
    let windowsEol = "\r\n"
    let macEol = "\r"

    let preparedText =
        text
            .Replace(windowsEol, unixEol)
            .Replace(macEol, unixEol)

    let separator = String.replicate (int numberOfEOLs) unixEol

    preparedText.Split(separator, StringSplitOptions.None)


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
            | isMatch -> isMatch
        )
        |> Seq.length

    numberOfLineEndings > 1

let DetectUnpinnedVersionsInGitHubCI(fileInfo: FileInfo) =
    assert (fileInfo.FullName.EndsWith(".yml"))

    let fileText = File.ReadAllText fileInfo.FullName

    let latestTagInRunsOnRegex =
        Regex("runs-on: .*-latest", RegexOptions.Compiled)

    latestTagInRunsOnRegex.IsMatch fileText

let DetectUnpinnedDotnetToolInstallVersions(fileInfo: FileInfo) =
    assert fileInfo.FullName.EndsWith ".yml"

    let fileLines = File.ReadLines fileInfo.FullName

    let dotnetToolInstallRegex =
        Regex("dotnet\\s+tool\\s+install\\s+", RegexOptions.Compiled)

    let unpinnedDotnetToolInstallVersions =
        fileLines
        |> Seq.filter dotnetToolInstallRegex.IsMatch
        |> Seq.filter(fun line ->
            not(line.Contains "--version") && not(line.Contains "-v")
        )
        |> (fun unpinnedVersions -> Seq.length unpinnedVersions > 0)

    unpinnedDotnetToolInstallVersions

let DetectUnpinnedNpmPackageInstallVersions(fileInfo: FileInfo) =
    assert fileInfo.FullName.EndsWith ".yml"

    let fileLines = File.ReadLines fileInfo.FullName

    let npmPackageInstallRegex =
        Regex("npm\\s+install\\s+", RegexOptions.Compiled)

    let npmPackageVersionRegex =
        Regex("@((\\d+\\.\\d+\\.\\d+)|(\\$[A-Z_]+))", RegexOptions.Compiled)

    let unpinnedNpmPackageInstallVersions =
        fileLines
        |> Seq.filter(fun line ->
            npmPackageInstallRegex.IsMatch line
            && npmPackageVersionRegex.IsMatch line |> not
        )
        |> (fun unpinnedVersions -> Seq.length unpinnedVersions > 0)

    unpinnedNpmPackageInstallVersions

let DetectAsteriskInPackageReferenceItems(fileInfo: FileInfo) =
    assert (fileInfo.FullName.EndsWith "proj")

    let fileText = File.ReadAllText fileInfo.FullName

    let asteriskInPackageReference =
        Regex(
            "<PackageReference.*Version=\".*\*.*\".*/>",
            RegexOptions.Compiled
        )

    asteriskInPackageReference.IsMatch fileText

let DetectMissingVersionsInNugetPackageReferences(fileInfo: FileInfo) =
    assert (fileInfo.FullName.EndsWith ".fsx")

    let fileLines = File.ReadLines fileInfo.FullName

    fileLines
    |> Seq.exists(fun line ->
        line.StartsWith "#r \"nuget:" && not(line.Contains ",")
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

        match Seq.tryLast filetext with
        | None
        | Some '\n' -> True
        | Some _ -> False

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
        |> Seq.filter(String.IsNullOrEmpty >> not)
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
        |> Seq.collect(fun paragraph ->
            if paragraph.Type = CodeBlock then
                Seq.singleton paragraph
            else
                let singleEolToJustSeparateLines = 1u

                let lines =
                    SplitByEOLs paragraph.Text singleEolToJustSeparateLines

                lines
                |> Seq.collect(fun line ->
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
        )

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
    let twoEolsToSeparateParagraphs = 2u

    let wrappedParagraphs =
        SplitByEOLs text twoEolsToSeparateParagraphs
        |> Seq.map(fun paragraph -> WrapParagraph paragraph maxCharsPerLine)

    String.Join(
        $"{Environment.NewLine}{Environment.NewLine}",
        wrappedParagraphs
    )

let private GetVersionsMapFromFiles
    (fileInfos: seq<FileInfo>)
    (versionRegexPattern: string)
    : Map<string, Set<string>> =
    let versionRegex = Regex(versionRegexPattern, RegexOptions.Compiled)

    let allFilesTexts =
        fileInfos
        |> Seq.map(fun fileInfo -> File.ReadAllText fileInfo.FullName)
        |> String.concat Environment.NewLine

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

let private DetectInconsistentVersionsInYamlFiles
    (fileInfos: seq<FileInfo>)
    (extractVersionsFunction: YamlNode -> seq<string * string>)
    =
    let envVarRegex =
        Regex(@"\s*\$\{\{\s*([^\s\}]+)\s*\}\}\s*", RegexOptions.Compiled)

    let yamlDocuments =
        Seq.map
            (fun (fileInfo: FileInfo) ->
                let yaml = YamlStream()
                use reader = new StreamReader(fileInfo.FullName)
                yaml.Load reader
                yaml.Documents[0].RootNode
            )
            fileInfos

    let versionMap =
        Seq.fold
            (fun mapping (yamlDoc: YamlNode) ->
                let matches =
                    Seq.collect extractVersionsFunction yamlDoc.AllNodes

                matches
                |> Seq.fold
                    (fun acc (key, value) ->
                        let actualValue =
                            let variableRegexMatch = envVarRegex.Match value

                            if variableRegexMatch.Success then
                                let yamlDict = yamlDoc :?> YamlMappingNode

                                let envDict =
                                    yamlDict.Children.["env"]
                                    :?> YamlMappingNode

                                let referenceString =
                                    variableRegexMatch.Groups.[1].Value

                                let envVarName =
                                    if referenceString.StartsWith "env." then
                                        referenceString.[4..]
                                    else
                                        referenceString

                                match envDict.Children.TryGetValue envVarName
                                    with
                                | true, envVarValue ->
                                    (envVarValue :?> YamlScalarNode).Value
                                | false, _ ->
                                    failwithf "env. var %s not found" envVarName
                            else
                                value

                        match Map.tryFind key acc with
                        | Some prevSet ->
                            Map.add key (Set.add actualValue prevSet) acc
                        | None -> Map.add key (Set.singleton actualValue) acc
                    )
                    mapping
            )
            Map.empty
            yamlDocuments

    versionMap
    |> Seq.map(fun item -> Seq.length item.Value > 1)
    |> Seq.contains true

let DetectInconsistentVersionsInGitHubCIWorkflow(fileInfos: seq<FileInfo>) =
    fileInfos
    |> Seq.iter(fun fileInfo -> assert (fileInfo.FullName.EndsWith ".yml"))

    let extractVersions(node: YamlNode) =
        match node with
        | :? YamlMappingNode as yamlDict ->
            yamlDict.Children
            |> Seq.collect(fun (KeyValue(keyNode, valueNode)) ->
                match keyNode, valueNode with
                | (:? YamlScalarNode as keyScalar),
                  (:? YamlScalarNode as valueScalar) when
                    keyScalar.Value = "uses"
                    ->
                    match valueScalar.Value.Split "@v" with
                    | [| name; version |] -> Seq.singleton(name, version)
                    | _ -> Seq.empty
                | (:? YamlScalarNode as keyScalar),
                  (:? YamlMappingNode as valueMapping) when
                    keyScalar.Value = "with"
                    ->
                    valueMapping.Children
                    |> Seq.choose(fun (KeyValue(innerKeyNode, innerValueNode)) ->
                        match innerKeyNode, innerValueNode with
                        | (:? YamlScalarNode as keyScalar),
                          (:? YamlScalarNode as valueScalar) ->
                            match keyScalar.Value.Split '-' with
                            | [| name; "version" |] ->
                                Some(name, valueScalar.Value)
                            | _ -> None
                        | _ -> None
                    )
                | _ -> Seq.empty
            )
        | _ -> Seq.empty

    DetectInconsistentVersionsInYamlFiles fileInfos extractVersions

let DetectInconsistentVersionsInGitHubCI(dir: DirectoryInfo) =
    let ymlFiles = dir.GetFiles("*.yml", SearchOption.AllDirectories)

    if Seq.isEmpty ymlFiles then
        false
    else
        DetectInconsistentVersionsInGitHubCIWorkflow ymlFiles

let GetVersionsMapForNugetRefsInFSharpScripts(fileInfos: seq<FileInfo>) =
    fileInfos
    |> Seq.iter(fun fileInfo -> assert (fileInfo.FullName.EndsWith ".fsx"))

    let versionRegexPattern =
        "#r \"nuget:\\s*([^\\s]*)\\s*,\\s*Version\\s*=\\s*([^\\s]*)\\s*\""

    GetVersionsMapFromFiles fileInfos versionRegexPattern

let DetectInconsistentVersionsInNugetRefsInFSharpScripts
    (fileInfos: seq<FileInfo>)
    =
    GetVersionsMapForNugetRefsInFSharpScripts fileInfos
    |> Map.exists(fun _ versions -> versions.Count > 1)

let DetectInconsistentVersionsInFSharpScripts
    (dir: DirectoryInfo)
    (ignoreDirs: Option<seq<string>>)
    =
    let fsxFiles =
        match ignoreDirs with
        | Some ignoreDirs ->
            dir.GetFiles("*.fsx", SearchOption.AllDirectories)
            |> Seq.filter(fun fileInfo ->
                ignoreDirs |> Seq.exists fileInfo.FullName.Contains |> not
            )
        | None -> dir.GetFiles("*.fsx", SearchOption.AllDirectories)

    if Seq.isEmpty fsxFiles then
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
        |> Seq.map fileInfo.FullName.EndsWith
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
                |> Seq.map line.Contains
                |> Seq.contains true

            nonVerboseFlag && not allowedNonVerboseFlag
        )
        |> Seq.length

    numInvalidFlags > 0

let IsExecutable(fileInfo: FileInfo) =
    let hasExecuteAccess = Syscall.access(fileInfo.FullName, AccessModes.X_OK)
    hasExecuteAccess = 0
