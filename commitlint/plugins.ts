import { abbr } from "./abbreviations";
import { Helpers } from "./helpers";

export abstract class Plugins {
    public static bodyProse(rawStr: string) {
        let offence = false;

        let lineBreakIndex = rawStr.indexOf("\n");

        if (lineBreakIndex >= 0) {
            // Extracting bodyStr from rawStr rather than using body directly is a
            // workaround for https://github.com/conventional-changelog/commitlint/issues/3412
            let bodyStr = rawStr.substring(lineBreakIndex);

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

                for (let paragraph of bodyStr.split("\n\n")) {
                    paragraph = paragraph.trim();

                    if (paragraph === "") {
                        continue;
                    }

                    let startWithLowerCase = Helpers.isLowerCase(paragraph[0]);

                    let validParagraphEnd = paragraphHasValidEnding(paragraph);

                    let lines = paragraph.split(/\r?\n/);

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
            `Please begin a paragraph with uppercase letter and end it with a dot.` +
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

            let titleWithoutArea = headerStr;
            if (colonIndex > 0) {
                titleWithoutArea = headerStr.substring(colonIndex);
            }

            let numRecomendations = 0;
            let lowerCaseTitleWithoutArea = titleWithoutArea.toLowerCase();
            Object.entries(abbr).forEach(([key, value]) => {
                let pattern = new RegExp("\\b(" + key.toString() + ")\\b");
                if (pattern.test(lowerCaseTitleWithoutArea)) {
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

    public static footerNotesMisplacement(rawStr: string) {
        let offence = false;

        let lineBreakIndex = rawStr.indexOf("\n");

        if (lineBreakIndex >= 0) {
            // Extracting bodyStr from rawStr rather than using body directly is a
            // workaround for https://github.com/conventional-changelog/commitlint/issues/3428
            let bodyStr = rawStr.substring(lineBreakIndex).trim();

            if (bodyStr !== "") {
                let seenBody = false;
                let seenFooter = false;
                let lines = bodyStr.split(/\r?\n/);
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
        }
        return [
            !offence,
            `Footer messages must be placed after body paragraphs, please move any message that starts with "Fixes", "Closes" or "[i]" to the end of the commmit message.` +
                Helpers.errMessageSuffix,
        ];
    }

    public static footerReferencesExistence(rawStr: string) {
        let offence = false;

        let lineBreakIndex = rawStr.indexOf("\n");

        if (lineBreakIndex >= 0) {
            // Extracting bodyStr from rawStr rather than using body directly is a
            // workaround for https://github.com/conventional-changelog/commitlint/issues/3428
            let bodyStr = rawStr.substring(lineBreakIndex).trim();

            if (bodyStr !== "") {
                let lines = bodyStr.split(/\r?\n/);
                let bodyReferences = new Set();
                let references = new Set();
                for (let line of lines) {
                    let matches = line.match(/(?<=\[)([0-9]+)(?=\])/g);
                    if (matches === null) {
                        continue;
                    }
                    for (let match of matches) {
                        if (Helpers.isFooterReference(line)) {
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
        }
        return [
            !offence,
            "All references in the body must be mentioned in the footer, and vice versa." +
                Helpers.errMessageSuffix,
        ];
    }

    public static preferSlashOverBackslash(headerStr: string) {
        let offence = false;

        let colonIndex = headerStr.indexOf(":");
        if (colonIndex >= 0) {
            let areaOrScope = headerStr.substring(0, colonIndex);
            if (areaOrScope.includes("\\")) {
                offence = true;
            }
        }

        return [
            !offence,
            `Please use slash instead of backslash in the area/scope/sub-area section of the title.` +
                Helpers.errMessageSuffix,
        ];
    }

    public static properIssueRefs(rawStr: string) {
        let offence = false;

        let lineBreakIndex = rawStr.indexOf("\n");

        if (lineBreakIndex >= 0) {
            // Extracting bodyStr from rawStr rather than using body directly is a
            // workaround for https://github.com/conventional-changelog/commitlint/issues/3412
            let bodyStr = rawStr.substring(lineBreakIndex);
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
            `Please start the title with an upper-case letter if there is no area in the title.` +
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
            `Please use "area/scope: subject" or "area(scope): subject" style instead of wrapping area/scope under square brackets in your commit message title` +
                Helpers.errMessageSuffix,
        ];
    }

    public static subjectLowercase(headerStr: string) {
        let offence = false;

        let colonFirstIndex = headerStr.indexOf(":");

        if (colonFirstIndex > 0 && headerStr.length > colonFirstIndex) {
            let subject = headerStr.substring(colonFirstIndex + 1).trim();
            if (subject != null && subject.length > 1) {
                let firstWord = subject.trim().split(" ")[0];
                offence = Helpers.wordIsStartOfSentence(firstWord);
            }
        }

        return [
            !offence,
            `Please use lowercase as the first letter for your subject, i.e. the text after your area/scope.` +
                Helpers.errMessageSuffix,
        ];
    }

    public static typeSpaceAfterComma(headerStr: string) {
        let offence = false;

        let colonIndex = headerStr.indexOf(":");

        if (colonIndex >= 0) {
            let areaOrScope = headerStr.substring(0, colonIndex);
            let commaIndex = areaOrScope.indexOf(",");
            while (commaIndex >= 0) {
                if (areaOrScope[commaIndex + 1] === " ") {
                    offence = true;
                }
                areaOrScope = areaOrScope.substring(commaIndex + 1);
                commaIndex = areaOrScope.indexOf(",");
            }
        }

        return [
            !offence,
            `No need to use space after comma in the area/scope (so that commit title can be shorter).` +
                Helpers.errMessageSuffix,
        ];
    }

    public static bodySoftMaxLineLength(
        rawStr: string,
        bodyMaxLineLength: number
    ) {
        let offence = false;

        let lineBreakIndex = rawStr.indexOf("\n");

        if (lineBreakIndex >= 0) {
            // Extracting bodyStr from rawStr rather than using body directly is a
            // workaround for https://github.com/conventional-changelog/commitlint/issues/3428
            let bodyStr = rawStr.substring(lineBreakIndex);

            bodyStr = Helpers.removeAllCodeBlocks(bodyStr).trim();

            if (bodyStr !== "") {
                let lines = bodyStr.split(/\r?\n/);
                let inBigBlock = false;
                for (let line of lines) {
                    if (Helpers.isBigBlock(line)) {
                        inBigBlock = !inBigBlock;
                        continue;
                    }
                    if (inBigBlock) {
                        continue;
                    }
                    if (line.length > bodyMaxLineLength) {
                        let isUrl = Helpers.isValidUrl(line);

                        let lineIsFooterNote = Helpers.isFooterNote(line);

                        if (!isUrl && !lineIsFooterNote) {
                            offence = true;
                            break;
                        }
                    }
                }
            }
        }

        // taken from https://stackoverflow.com/a/66433444/544947 and https://unix.stackexchange.com/a/25208/56844
        function getUnixCommand(fmtOption: string) {
            return `git log --format=%B -n 1 $(git log -1 --pretty=format:"%h") | cat - > log.txt ; fmt -w 1111 -s log.txt > ulog.txt && fmt -w 64 -s ${fmtOption} ulog.txt > wlog.txt && git commit --amend -F wlog.txt`;
        }

        return [
            !offence,
            `Please do not exceed ${bodyMaxLineLength} characters in the lines of the commit message's body; we recommend this unix command (for editing the last commit message): \n` +
                `For Linux users: ${getUnixCommand("-u")}\n` +
                `For macOS users: ${getUnixCommand("")}` +
                Helpers.errMessageSuffix,
        ];
    }

    public static trailingWhitespace(rawStr: string) {
        let offence = false;

        let lines = rawStr.split(/\r?\n/);
        let inBigBlock = false;
        for (let line of lines) {
            if (Helpers.isBigBlock(line)) {
                inBigBlock = !inBigBlock;
                continue;
            }
            if (inBigBlock) {
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
            let areaOrScope = headerStr.substring(0, colonIndex);
            let parenIndex = areaOrScope.indexOf("(");
            if (parenIndex >= 1) {
                if (headerStr[parenIndex - 1] === " ") {
                    offence = true;
                }
            }
        }

        return [
            !offence,
            `No need to use space before parentheses in the area/scope/sub-area section of the title.` +
                Helpers.errMessageSuffix,
        ];
    }
}
