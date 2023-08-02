import { abbr } from "./abbreviations";
import { Helpers } from "./helpers";

export abstract class Plugins {
    public static bodyProse(rawStr: string) {
        let offence = false;

        rawStr = rawStr.trim();
        let lineBreakIndex = rawStr.indexOf("\n");

        if (lineBreakIndex >= 0) {
            // Extracting bodyStr from rawStr rather than using body directly is a
            // workaround for https://github.com/conventional-changelog/commitlint/issues/3412
            let bodyStr = rawStr.substring(lineBreakIndex).trim();

            bodyStr = Helpers.removeAllCodeBlocks(bodyStr).trim();

            if (bodyStr !== "") {
                function paragraphHasValidEnding(paragraph: string): boolean {
                    let paragraphWords = paragraph.split(" ");
                    let lastWordInParagraph =
                        paragraphWords[paragraphWords.length - 1];
                    let isParagraphEndingWithUrl =
                        Helpers.isValidUrl(lastWordInParagraph);
                    if (isParagraphEndingWithUrl) {
                        return true;
                    }

                    let endingChar = paragraph[paragraph.length - 1];
                    if (
                        endingChar === "." ||
                        endingChar === ":" ||
                        endingChar === "!" ||
                        endingChar === "?"
                    ) {
                        return true;
                    }
                    if (
                        endingChar === ")" &&
                        paragraph.length > 1 &&
                        paragraphHasValidEnding(paragraph[paragraph.length - 2])
                    ) {
                        return true;
                    }
                    return false;
                }

                for (let paragraph of Helpers.splitByEOLs(bodyStr, 2)) {
                    paragraph = paragraph.trim();

                    if (paragraph === "") {
                        continue;
                    }

                    let startWithLowerCase = Helpers.isLowerCase(paragraph[0]);

                    let validParagraphEnd = paragraphHasValidEnding(paragraph);

                    let lines = Helpers.splitByEOLs(paragraph, 1);

                    if (startWithLowerCase) {
                        if (
                            !(lines.length == 1 && Helpers.isValidUrl(lines[0]))
                        ) {
                            offence = true;
                        }
                    }

                    if (
                        !validParagraphEnd &&
                        !Helpers.isValidUrl(lines[lines.length - 1]) &&
                        !Helpers.isFooterNote(lines[lines.length - 1])
                    ) {
                        offence = true;
                    }
                }
            }
        }

        return [
            !offence,
            `The body of the commit message (as opposed to the commit message title) is composed of paragraphs. Please begin each paragraph with an uppercase letter and end it with a dot (or other valid character to finish a paragraph).` +
                Helpers.errMessageSuffix,
        ];
    }

    public static commitHashAlone(rawStr: string) {
        let offence = false;

        let urls = Helpers.findUrls(rawStr);

        let gitRepo = process.env["GITHUB_REPOSITORY"];
        if (gitRepo !== undefined && urls !== null) {
            for (let url of urls.entries()) {
                let urlStr = url[1].toString();
                if (Helpers.isCommitUrl(urlStr) && urlStr.includes(gitRepo)) {
                    offence = true;
                    break;
                }
            }
        }

        return [
            !offence,
            `Please use the commit hash instead of the commit full URL.` +
                Helpers.errMessageSuffix,
        ];
    }

    public static emptyWip(headerStr: string) {
        let offence = headerStr.toLowerCase() === "wip";
        return [
            !offence,
            `Please add a number or description after the WIP prefix.` +
                Helpers.errMessageSuffix,
        ];
    }

    public static headerMaxLengthWithSuggestions(
        headerStr: string,
        maxLineLength: number
    ) {
        let offence = false;

        let headerLength = headerStr.length;
        let message = `Please do not exceed ${maxLineLength} characters in title (found ${headerLength}).`;
        if (!headerStr.startsWith("Merge ") && headerLength > maxLineLength) {
            offence = true;

            let colonIndex = headerStr.indexOf(":");

            let titleWithoutScope = headerStr;
            if (colonIndex > 0) {
                titleWithoutScope = headerStr.substring(colonIndex);
            }

            let numRecomendations = 0;
            let lowerCaseTitleWithoutScope = titleWithoutScope.toLowerCase();
            Object.entries(abbr).forEach(([key, value]) => {
                let pattern = new RegExp("\\b(" + key.toString() + ")\\b");
                if (pattern.test(lowerCaseTitleWithoutScope)) {
                    if (numRecomendations === 0) {
                        message =
                            message +
                            " The following replacement(s) in your commit title are recommended:\n";
                    }

                    message = message + `"${key}" -> "${value}"\n`;
                }
            });
        }

        return [!offence, message + Helpers.errMessageSuffix];
    }

    public static footerNotesMisplacement(bodyStr: string | null) {
        let offence = false;

        if (bodyStr !== null) {
            bodyStr = Helpers.removeAllCodeBlocks(bodyStr).trim();
            let seenBody = false;
            let seenFooter = false;
            let lines = Helpers.splitByEOLs(bodyStr, 1);
            for (let line of lines) {
                if (line.length === 0) {
                    continue;
                }
                seenBody = seenBody || !Helpers.isFooterNote(line);
                seenFooter = seenFooter || Helpers.isFooterNote(line);
                if (seenFooter && !Helpers.isFooterNote(line)) {
                    offence = true;
                    break;
                }
            }
        }

        return [
            !offence,
            `Footer messages must be placed after body paragraphs, please move any message that starts with "Fixes", "Closes" or "[i]" to the end of the commmit message.` +
                Helpers.errMessageSuffix,
        ];
    }

    public static footerRefsValidity(rawStr: string) {
        let offence = false;
        let hasEmptyFooter = false;

        rawStr = rawStr.trim();
        let lineBreakIndex = rawStr.indexOf("\n");

        if (lineBreakIndex >= 0) {
            // Extracting bodyStr from rawStr rather than using body directly is a
            // workaround for https://github.com/conventional-changelog/conventional-changelog/issues/1016
            let bodyStr = rawStr.substring(lineBreakIndex).trim();
            bodyStr = Helpers.removeAllCodeBlocks(bodyStr);

            let lines = Helpers.splitByEOLs(bodyStr, 1);
            let bodyReferences = new Set();
            let references = new Set();
            for (let line of lines) {
                let matches = line.match(/(?<=\[)([0-9]+)(?=\])/g);
                if (matches === null) {
                    continue;
                }
                for (let match of matches) {
                    if (Helpers.isEmptyFooterReference(line)) {
                        offence = true;
                        hasEmptyFooter = true;
                    } else if (Helpers.isFooterReference(line)) {
                        references.add(match);
                    } else {
                        bodyReferences.add(match);
                    }
                }
            }
            for (let ref of bodyReferences) {
                if (!references.has(ref)) {
                    offence = true;
                    break;
                }
            }
            for (let ref of references) {
                if (!bodyReferences.has(ref)) {
                    offence = true;
                    break;
                }
            }
        }

        let errorMessage =
            "All references in the body must be mentioned in the footer, and vice versa.";

        if (hasEmptyFooter) {
            errorMessage =
                "A footer reference can not be empty, please make sure that you've provided the reference and there is no EOL between the reference number and the reference.";
        }

        return [!offence, errorMessage + Helpers.errMessageSuffix];
    }

    public static preferSlashOverBackslash(headerStr: string) {
        let offence = false;

        let colonIndex = headerStr.indexOf(":");
        if (colonIndex >= 0) {
            let scope = headerStr.substring(0, colonIndex);
            if (scope.includes("\\")) {
                offence = true;
            }
        }

        return [
            !offence,
            `Please use slash instead of backslash in the scope/sub-scope section of the title.` +
                Helpers.errMessageSuffix,
        ];
    }

    public static properIssueRefs(rawStr: string) {
        let offence = false;

        rawStr = rawStr.trim();
        let lineBreakIndex = rawStr.indexOf("\n");

        if (lineBreakIndex >= 0) {
            // Extracting bodyStr from rawStr rather than using body directly is a
            // workaround for https://github.com/conventional-changelog/commitlint/issues/3412
            let bodyStr = rawStr.substring(lineBreakIndex).trim();

            bodyStr = Helpers.removeAllCodeBlocks(bodyStr);
            offence = Helpers.includesHashtagRef(bodyStr);
        }

        return [
            !offence,
            `Please use full URLs instead of #XYZ refs.` +
                Helpers.errMessageSuffix,
        ];
    }

    public static titleUppercase(headerStr: string) {
        let firstWord = headerStr.split(" ")[0];
        let offence =
            headerStr.indexOf(":") < 0 &&
            !Helpers.wordIsStartOfSentence(firstWord) &&
            !Helpers.isProperNoun(firstWord);
        return [
            !offence,
            `Please start the title with an uppercase letter if you haven't specified any scope.` +
                Helpers.errMessageSuffix,
        ];
    }

    public static tooManySpaces(rawStr: string) {
        rawStr = Helpers.removeAllCodeBlocks(rawStr);
        let offence = rawStr.match(`[^.]  `) !== null;

        return [
            !offence,
            `Please watch out for too many whitespaces in the text.` +
                Helpers.errMessageSuffix,
        ];
    }

    public static typeSpaceAfterColon(headerStr: string) {
        let colonFirstIndex = headerStr.indexOf(":");

        let offence = false;
        if (colonFirstIndex > 0 && headerStr.length > colonFirstIndex) {
            if (headerStr[colonFirstIndex + 1] != " ") {
                offence = true;
            }
        }

        return [
            !offence,
            `Please place a space after the first colon character in your commit message title` +
                Helpers.errMessageSuffix,
        ];
    }

    public static typeWithSquareBrackets(headerStr: string) {
        let offence = headerStr.match(`^\\[.*\\]`) !== null;

        return [
            !offence,
            `Please use "scope/sub-scope: subject" or "scope(scope): subject" style instead of wrapping the scope details under square brackets in your commit message title` +
                Helpers.errMessageSuffix,
        ];
    }

    public static subjectLowercase(headerStr: string) {
        let offence = false;

        let colonFirstIndex = headerStr.indexOf(":");

        let firstWord = "";
        if (colonFirstIndex > 0 && headerStr.length > colonFirstIndex) {
            let subject = headerStr.substring(colonFirstIndex + 1).trim();
            if (subject != null && subject.length > 1) {
                firstWord = subject.trim().split(" ")[0];
                offence = Helpers.wordIsStartOfSentence(firstWord);
            }
        }

        return [
            !offence,
            `Please use lowercase as the first letter for your subject, i.e. the text after your scope (note: there is a chance that this rule is yielding a false positive in case the word '${firstWord}' is a name and must be capitalized; in which case please just reword the subject to make this word not be the first, sorry).` +
                Helpers.errMessageSuffix,
        ];
    }

    public static typeSpaceAfterComma(headerStr: string) {
        let offence = false;

        let colonIndex = headerStr.indexOf(":");

        if (colonIndex >= 0) {
            let scope = headerStr.substring(0, colonIndex);
            let commaIndex = scope.indexOf(",");
            while (commaIndex >= 0) {
                if (scope[commaIndex + 1] === " ") {
                    offence = true;
                }
                scope = scope.substring(commaIndex + 1);
                commaIndex = scope.indexOf(",");
            }
        }

        return [
            !offence,
            `No need to use space after comma in the scope (so that commit title can be shorter).` +
                Helpers.errMessageSuffix,
        ];
    }

    public static bodySoftMaxLineLength(
        bodyStr: string | null,
        bodyMaxLineLength: number
    ) {
        let offence = false;

        if (bodyStr !== null) {
            bodyStr = bodyStr.trim();
            bodyStr = Helpers.removeAllCodeBlocks(bodyStr).trim();

            if (bodyStr !== "") {
                let lines = Helpers.splitByEOLs(bodyStr, 1);
                let inCodeBlock = false;
                for (let line of lines) {
                    if (Helpers.isCodeBlockDelimiter(line)) {
                        inCodeBlock = !inCodeBlock;
                        continue;
                    }
                    if (inCodeBlock) {
                        continue;
                    }
                    if (line.length > bodyMaxLineLength) {
                        let isUrl = Helpers.isValidUrl(line);

                        let lineIsFooterNote = Helpers.isFooterNote(line);

                        let commitHashPattern = `([0-9a-f]{40})`;
                        let anySinglePunctuationCharOrNothing = `[\.\,\:\;\?\!]?`;
                        let index = line.search(
                            commitHashPattern +
                                anySinglePunctuationCharOrNothing +
                                `$`
                        );
                        let endsWithCommitHashButRestIsNotTooLong =
                            index != -1 && index < bodyMaxLineLength;

                        if (
                            !isUrl &&
                            !lineIsFooterNote &&
                            !endsWithCommitHashButRestIsNotTooLong
                        ) {
                            offence = true;
                            break;
                        }
                    }
                }
            }
        }

        return [
            !offence,
            `Please do not exceed ${bodyMaxLineLength} characters in the lines of the commit message's body; we recommend this script (for editing the last commit message): \n` +
                "https://github.com/nblockchain/conventions/blob/master/scripts/wrapLatestCommitMsg.fsx" +
                Helpers.errMessageSuffix,
        ];
    }

    public static bodyParagraphLineMinLength(
        bodyStr: string | null,
        paragraphLineMinLength: number,
        paragraphLineMaxLength: number
    ) {
        let offence = false;

        if (bodyStr !== null) {
            bodyStr = Helpers.removeAllCodeBlocks(bodyStr).trim();

            let paragraphs = Helpers.splitByEOLs(bodyStr, 2);
            for (let paragraph of paragraphs) {
                let lines = Helpers.splitByEOLs(paragraph, 1);

                // NOTE: we don't iterate over the last line, on purpose
                for (let i = 0; i < lines.length - 1; i++) {
                    let line = lines[i];

                    if (line.length == 0) {
                        continue;
                    }

                    if (line.length < paragraphLineMinLength) {
                        // this ref doesn't go out of bounds because we didn't iter on last line
                        let nextLine = lines[i + 1];

                        let isUrl =
                            Helpers.isValidUrl(line) ||
                            Helpers.isValidUrl(nextLine);

                        let lineIsFooterNote = Helpers.isFooterNote(line);

                        let nextWordLength = lines[i + 1].split(" ")[0].length;
                        let isNextWordTooLong =
                            nextWordLength + line.length + 1 >
                            paragraphLineMaxLength;

                        let isLastCharAColonBreak =
                            line[line.length - 1] === ":" &&
                            nextLine[0].toUpperCase() == nextLine[0];

                        if (
                            !isUrl &&
                            !lineIsFooterNote &&
                            !isNextWordTooLong &&
                            !isLastCharAColonBreak
                        ) {
                            offence = true;
                            break;
                        }
                    }
                }
            }
        }

        return [
            !offence,
            `Please do not subceed ${paragraphLineMinLength} characters in the lines of the commit message's body paragraphs (except the last line of each paragraph); we recommend this script (for editing the last commit message): \n` +
                "https://github.com/nblockchain/conventions/blob/master/scripts/wrapLatestCommitMsg.fsx" +
                Helpers.errMessageSuffix,
        ];
    }

    public static trailingWhitespace(rawStr: string) {
        let offence = false;

        let lines = Helpers.splitByEOLs(rawStr, 1);
        let inCodeBlock = false;
        for (let line of lines) {
            if (Helpers.isCodeBlockDelimiter(line)) {
                inCodeBlock = !inCodeBlock;
                continue;
            }
            if (inCodeBlock) {
                continue;
            }

            if (line[0] == " " || line[0] == "\t") {
                offence = true;
                break;
            }

            if (line.length > 0) {
                let lastChar = line[line.length - 1];
                if (lastChar == " " || lastChar == "\t") {
                    offence = true;
                    break;
                }
            }
        }

        return [
            !offence,
            `Please watch out for leading or ending trailing whitespace.` +
                Helpers.errMessageSuffix,
        ];
    }

    public static typeSpaceBeforeParen(headerStr: string) {
        let offence = false;

        let colonIndex = headerStr.indexOf(":");
        if (colonIndex >= 0) {
            let scope = headerStr.substring(0, colonIndex);
            let parenIndex = scope.indexOf("(");
            if (parenIndex >= 1) {
                if (headerStr[parenIndex - 1] === " ") {
                    offence = true;
                }
            }
        }

        return [
            !offence,
            `No need to use space before parentheses in the scope/sub-scope section of the title.` +
                Helpers.errMessageSuffix,
        ];
    }
}
