module FileConventions

open System
open System.IO
open System.Linq
open System.Text.RegularExpressions

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
        |> Seq.filter(fun line -> dotnetToolInstallRegex.IsMatch line)
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
        Regex("@\\d+\\.\\d+\\.\\d+", RegexOptions.Compiled)

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
    |> Seq.iter(fun fileInfo -> assert (fileInfo.FullName.EndsWith ".yml"))

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
    |> Seq.iter(fun fileInfo -> assert (fileInfo.FullName.EndsWith ".fsx"))

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
