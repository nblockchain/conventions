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
    use streamReader = new StreamReader(fileInfo.FullName)
    let fileText = streamReader.ReadToEnd()

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
    use streamReader = new StreamReader(fileInfo.FullName)
    let fileText = streamReader.ReadToEnd()

    let latestTagInRunsOnRegex =
        Regex("runs-on: .*-latest", RegexOptions.Compiled)

    latestTagInRunsOnRegex.IsMatch fileText

let DetectAsteriskInPackageReferenceItems(fileInfo: FileInfo) =
    assert (fileInfo.FullName.EndsWith "proj")
    use streamReader = new StreamReader(fileInfo.FullName)
    let fileText = streamReader.ReadToEnd()

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

    let mutable currentLine = ""
    let mutable wrappedText = ""

    for word in words do
        if String.IsNullOrEmpty currentLine then
            currentLine <- word.Text
        elif word.Type <> PlainText then
            wrappedText <-
                wrappedText
                + currentLine
                + Environment.NewLine
                + word.Text
                + Environment.NewLine

            currentLine <- ""
        elif String.length currentLine + word.Text.Length + 1 <= maxCharsPerLine then
            currentLine <- currentLine + " " + word.Text
        else
            wrappedText <- wrappedText + currentLine + Environment.NewLine
            currentLine <- word.Text

    (wrappedText + currentLine).Trim()

let WrapText (text: string) (maxCharsPerLine: int) : string =
    let wrappedParagraphs =
        text.Split $"{Environment.NewLine}{Environment.NewLine}"
        |> Seq.map(fun paragraph -> WrapParagraph paragraph maxCharsPerLine)

    String.Join(
        $"{Environment.NewLine}{Environment.NewLine}",
        wrappedParagraphs
    )
