export abstract class Helpers {
    public static errMessageSuffix =
        "\nFor reference, these are the guidelines that include our commit message conventions: https://github.com/nblockchain/conventions/blob/master/WorkflowGuidelines.md";

    public static isValidUrl(text: string) {
        if (text.indexOf(" ") >= 0) {
            return false;
        }

        // Borrowed from https://www.freecodecamp.org/news/check-if-a-javascript-string-is-a-url/
        try {
            return Boolean(new URL(text));
        } catch (e) {
            return false;
        }
    }

    // to convert from 'any' type
    public static convertAnyToString(
        potentialString: any,
        paramName: string
    ): string | null {
        if (potentialString === null || potentialString === undefined) {
            // otherwise, String(null) might give us the stupid string "null"
            return null;
        }
        return String(potentialString);
    }

    public static assertNotNull(
        text: string | null,
        errorMessage: string
    ): string {
        if (text === null) {
            throw new Error(errorMessage);
        }
        return text as string;
    }

    public static assertCharacter(letter: string) {
        if (letter.length !== 1) {
            throw Error("This function expects a character as input");
        }
    }

    public static assertLine(line: string) {
        if (line.includes("\n")) {
            throw Error("This function expects a line as input");
        }
    }

    public static assertWord(word: string) {
        if (word.includes("\n") || word.includes(" ")) {
            throw Error(
                "This function expects a word as input.\n" +
                    "A word doesn't include line breaks and whitespaces."
            );
        }
    }

    public static assertHigherThanZero(number: number) {
        if (number < 1) {
            throw Error("This param cannot be negative or zero: " + number);
        }
    }

    public static assertUrl(url: string) {
        if (!Helpers.isValidUrl(url)) {
            throw Error("This function expects a url as input");
        }
    }

    public static findUrls(text: string) {
        var urlRegex = /(https?:\/\/[^\s]+)/g;
        return text.match(urlRegex);
    }

    public static isCommitUrl(url: string) {
        Helpers.assertUrl(url);
        return url.includes("/commit/");
    }

    public static isCodeBlockDelimiter(line: string) {
        Helpers.assertLine(line);
        let codeBlockDelimiter = "```";
        return line == codeBlockDelimiter;
    }

    public static isUpperCase(letter: string) {
        Helpers.assertCharacter(letter);
        let isUpperCase = letter.toUpperCase() == letter;
        let isLowerCase = letter.toLowerCase() == letter;

        return isUpperCase && !isLowerCase;
    }

    public static isLowerCase(letter: string) {
        Helpers.assertCharacter(letter);
        let isUpperCase = letter.toUpperCase() == letter;
        let isLowerCase = letter.toLowerCase() == letter;

        return isLowerCase && !isUpperCase;
    }

    public static isEmptyFooterReference(line: string) {
        Helpers.assertLine(line);
        let trimmedLine = line.trim();
        return (
            trimmedLine[0] === "[" &&
            trimmedLine.indexOf("]") === trimmedLine.length - 1
        );
    }

    public static isFooterReference(line: string) {
        Helpers.assertLine(line);
        return line[0] === "[" && line.indexOf("] ") > 0;
    }

    public static isFixesOrClosesSentence(line: string) {
        Helpers.assertLine(line);
        return line.indexOf("Fixes ") == 0 || line.indexOf("Closes ") == 0;
    }

    public static isCoAuthoredByTag(line: string) {
        Helpers.assertLine(line);
        return line.indexOf("Co-authored-by: ") == 0;
    }

    public static isFooterNote(line: string): boolean {
        Helpers.assertLine(line);
        return (
            Helpers.isFooterReference(line) ||
            Helpers.isCoAuthoredByTag(line) ||
            Helpers.isFixesOrClosesSentence(line)
        );
    }

    public static numUpperCaseLetters(word: string) {
        Helpers.assertWord(word);
        return word.length - word.replace(/[A-Z]/g, "").length;
    }

    public static numNonAlphabeticalCharacters(word: string) {
        Helpers.assertWord(word);
        return word.length - word.replace(/[^a-zA-Z]/g, "").length;
    }

    public static isProperNoun(word: string) {
        Helpers.assertWord(word);
        let numUpperCase = Helpers.numUpperCaseLetters(word);
        let numNonAlphabeticalChars =
            Helpers.numNonAlphabeticalCharacters(word);

        return (
            numNonAlphabeticalChars > 0 ||
            (Helpers.isUpperCase(word[0]) && numUpperCase > 1) ||
            (Helpers.isLowerCase(word[0]) && numUpperCase > 0)
        );
    }

    public static wordIsStartOfSentence(word: string) {
        Helpers.assertWord(word);
        if (Helpers.isUpperCase(word[0])) {
            let numUpperCase = Helpers.numUpperCaseLetters(word);
            let numNonAlphabeticalChars =
                Helpers.numNonAlphabeticalCharacters(word);
            return numUpperCase == 1 && numNonAlphabeticalChars == 0;
        }
        return false;
    }

    public static includesHashtagRef(text: string) {
        return (
            text.match(`\\s+#[0-9]+`) !== null ||
            text.match(`^#[0-9]+`) !== null
        );
    }

    public static removeAllCodeBlocks(text: string) {
        return text.replace(/```[^]*```/g, "");
    }

    public static splitByEOLs(text: string, numberOfEols: number) {
        Helpers.assertHigherThanZero(numberOfEols);

        let unixEol = "\n";
        let windowsEol = "\r\n";
        let macEol = "\r";

        let preparedText = text
            .replaceAll(windowsEol, unixEol)
            .replaceAll(macEol, unixEol);

        let separator = "";
        do {
            separator += unixEol;
            numberOfEols--;
        } while (numberOfEols != 0);

        return preparedText.split(separator);
    }
}
