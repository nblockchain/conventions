// FIXME: move abbr below to separate file when we start using
// the whole repo instead of just wget'ing this single file
let abbr = {
    "1 second": "1sec",
    "1second": "1sec",
    "absolute": "abs",
    "address": "addr",
    "allocate": "alloc",
    "alternate": "alt",
    "alternative": "alt",
    "and": "&",
    "application": "app",
    "argument": "arg",
    "asynchronous": "async",
    "attribute": "attrib",
    "authenticate": "auth",
    "authentication": "auth",
    "average": "avg",
    "background": "bg",
    "binary": "bin",
    "bitcoin": "BTC",
    "block": "blk",
    "boolean": "bool",
    "buffer": "buf",
    "button": "btn",
    "calculate": "calc",
    "callback": "cb",
    "channel": "chan",
    "character": "char",
    "collection": "coll",
    "column": "col",
    "command": "cmd",
    "command line": "cmdline",
    "compare": "cmp",
    "concatenate": "concat",
    "config": "cfg",
    "configuration": "config",
    "connection": "conn",
    "context": "ctx",
    "continue": "cont",
    "control": "ctrl",
    "conversation": "convo",
    "conversion": "conv",
    "convert": "conv",
    "coordinate": "coord",
    "database": "db",
    "debug": "dbg",
    "decimal": "dec",
    "decrease": "dec",
    "define": "def",
    "delete": "drop",
    "deprecated": "obsolete",
    "destination": "dest",
    "developer": "dev",
    "development": "dev",
    "difference": "diff",
    "directory": "dir",
    "document": "doc",
    "dollar": "USD",
    "dollars": "USD",
    "dos to unix": "dos2unix",
    "dotnet": ".NET",
    "eight": "8",
    "end to end": "end2end",
    "end2end": "e2e",
    "english": "eng",
    "environment": "env",
    "error": "err",
    "ethereum": "ETH",
    "executable": "exe",
    "execute": "exec",
    "expression": "expr",
    "figure": "fig",
    "first": "1st",
    "five": "5",
    "folder": "dir",
    "for example": "e.g.",
    "format": "fmt",
    "four": "4",
    "func": "fn",
    "function": "func",
    "generate": "gen",
    "generation": "gen",
    "hardware": "hw",
    "hexadecimal": "hex",
    "identifier": "id",
    "image": "img",
    "increase": "inc",
    "increment": "inc",
    "index": "idx",
    "information": "info",
    "initialize": "init",
    "integer": "int",
    "interface": "iface",
    "language": "lang",
    "length": "len",
    "library": "lib",
    "litecoin": "LTC",
    "macOS": "mac",
    "maximum": "max",
    "memory": "mem",
    "message": "msg",
    "middle": "mid",
    "minimum": "min",
    "minute": "min",
    "minutes": "mins",
    "miscellaneous": "misc",
    "modulo": "mod",
    "navigation": "nav",
    "network": "net",
    "nine": "9",
    "no/yes": "y/n",
    "number": "num",
    "object": "obj",
    "one": "1",
    "one second": "1sec",
    "operating system": "OS",
    "operation": "op",
    "optional": "opt",
    "order by": "sort by",
    "parameter": "param",
    "parentheses": "parens",
    "parenthesis": "paren",
    "picture": "pic",
    "pointer": "ptr",
    "position": "pos",
    "power": "pow",
    "preference": "pref",
    "previous": "prev",
    "problem": "issue",
    "process": "proc",
    "query": "qry",
    "range": "rng",
    "receive": "recv",
    "remove": "rm",
    "replace": "swap",
    "repository": "repo",
    "request": "req",
    "result": "res",
    "return": "ret",
    "revision": "rev",
    "second": "2nd",
    "seconds": "secs",
    "select": "sel",
    "sequence": "seq",
    "seven": "7",
    "six": "6",
    "software": "sw",
    "source": "src",
    "square root": "sqrt",
    "standard": "std",
    "statistic": "stat",
    "statistics": "stats",
    "string": "str",
    "synchronize": "sync",
    "temporary": "tmp",
    "third": "3rd",
    "three": "3",
    "timer": "tmr",
    "transaction": "tx",
    "transactions": "txs",
    "two": 2,
    "unix to dos": "unix2dos",
    "unnecessary": "unneeded",
    "unsigned integer": "uint",
    "value": "val",
    "variable": "var",
    "yes/no": "y/n",
    "zero": "0",
}

// to convert from 'any' type
function convertAnyToString(potentialString: any, paramName: string): string {
    if (potentialString === null || potentialString === undefined) {
        // otherwise, String(null) might give us the stupid string "null"
        throw new Error('Unexpected ' + paramName + '===null or ' + paramName + '===undefined happened');
    }
    return String(potentialString);
}

enum RuleStatus {
    Disabled = 0,
    Warning = 1,
    Error = 2,
}

let bodyMaxLineLength = 64;
let headerMaxLineLength = 50;

function isValidUrl(url: string) {
    if (url.indexOf(" ") > 0) {
        return false;
    }

    // Borrowed from https://www.freecodecamp.org/news/check-if-a-javascript-string-is-a-url/
    try { 
        return Boolean(new URL(url)); 
    }
    catch(e){ 
        return false; 
    }
}

function assertUrl(url: string) {
    if (!isValidUrl(url)) {
        throw Error('This function expects a url as input')   
    }
}

function assertCharacter(letter: string) {
    if (letter.length !== 1) {
        throw Error('This function expects a character as input')
    }
}

function assertLine(line: string) {
    if (line.includes('\n')) {
        throw Error('This function expects a line as input')
    }
}

function assertWord(word: string) {
    if (word.includes('\n') || word.includes(' ')) {
        throw Error("This function expects a word as input.\n" +
                    "A word doesn't include line breaks and whitespaces.")
    }
}

function isBigBlock(line: string) {
    assertLine(line);
    let bigBlockDelimiter = "```";
    return (line.length == bigBlockDelimiter.length) && (line.indexOf("```") == 0);
}

function isUpperCase(letter: string) {
    assertCharacter(letter);
    let isUpperCase = letter.toUpperCase() == letter;
    let isLowerCase = letter.toLowerCase() == letter;

    return (isUpperCase && !isLowerCase);
}

function isLowerCase(letter: string) {
    assertCharacter(letter);
    let isUpperCase = letter.toUpperCase() == letter;
    let isLowerCase = letter.toLowerCase() == letter;

    return (isLowerCase && !isUpperCase);
}

function isFooterReference(line: string) {
    assertLine(line);
    return (line[0] === "[" && line.indexOf("] ") > 0);
}

function isFixesSentence(line: string) {
    assertLine(line);
    return (line.indexOf("Fixes ") == 0);
}

function isCoAuthoredByTag(line: string) {
    assertLine(line);
    return (line.indexOf("Co-authored-by: ") == 0);
}

function isFooterNote(line: string): boolean {
    assertLine(line);
    return isFooterReference(line) ||
        isCoAuthoredByTag(line) ||
        isFixesSentence(line);
}

function wordIsStartOfSentence(word: string) {
    assertWord(word);
    if (isUpperCase(word[0])) {
        let numUpperCase = word.length - word.replace(/[A-Z]/g, '').length;
        let numNonAlphabeticalChars = word.length - word.replace(/[^a-zA-Z]/g, '').length
        return numUpperCase == 1 && numNonAlphabeticalChars == 0;
    }
    return false;
}

function includesHashtagRef(text: string) {
    return text.match(`#[0-9]+`) !== null;
}

function removeAllCodeBlocks(text: string) {
    return text.replace(/```[^]*```/g, '');
}

function findUrls(text: string) {
    var urlRegex = /(https?:\/\/[^\s]+)/g;
    return text.match(urlRegex);
}

function isCommitUrl(url: string) {
    assertUrl(url)
    return url.includes('/commit/');
}

module.exports = {
    parserPreset: 'conventional-changelog-conventionalcommits',
    rules: {
        'body-leading-blank': [RuleStatus.Warning, 'always'],
        'body-soft-max-line-length': [RuleStatus.Error, 'always'],
        'empty-wip': [RuleStatus.Error, 'always'],
        'footer-leading-blank': [RuleStatus.Warning, 'always'],
        'footer-max-line-length': [RuleStatus.Error, 'always', 150],
        'footer-notes-misplacement': [RuleStatus.Error, 'always'],
        'footer-references-existence': [RuleStatus.Error, 'always'],
        'header-max-length-with-suggestions': [RuleStatus.Error, 'always', headerMaxLineLength],
        'subject-full-stop': [RuleStatus.Error, 'never', '.'],
        'type-empty': [RuleStatus.Warning, 'never'],
        'type-space-after-colon': [RuleStatus.Error, 'always'],
        'subject-lowercase': [RuleStatus.Error, 'always'],
        'body-prose': [RuleStatus.Error, 'always'],
        'type-space-after-comma': [RuleStatus.Error, 'always'],
        'trailing-whitespace': [RuleStatus.Error, 'always'],
        'prefer-slash-over-backslash': [RuleStatus.Error, 'always'],
        'type-space-before-paren': [RuleStatus.Error, 'always'],
        'type-with-square-brackets': [RuleStatus.Error, 'always'],
        'proper-issue-refs': [RuleStatus.Error, 'always'],
        'too-many-spaces': [RuleStatus.Error, 'always'],
        'commit-hash-alone': [RuleStatus.Error, 'always'],
    },
    plugins: [
        // TODO (ideas for more rules):
        // * Better rule than body-max-line-length that ignores line if it starts with `[x] ` where x is a number.
        // * Detect if paragraphs in body have been cropped too shortly (less than 64 chars), and suggest same auto-wrap command that body-soft-max-line-length suggests, since it unwraps and wraps (both).
        // * Detect reverts which have not been elaborated.
        // * If full URL for commit found, reject in favour for just the commit hash.
        // * Reject some stupid obvious words: change, update, modify (if first word after colon, error; otherwise warning).
        // * Think of how to reject this shitty commit message: https://github.com/nblockchain/NOnion/pull/34/commits/9ffcb373a1147ed1c729e8aca4ffd30467255594
        // * Title should not have dot at the end.
        // * Second line of commit msg should always be blank.
        // * Workflow: detect if wip commit in a branch not named "wip/*" or whose name contains "squashed".
        // * Detect if commit hash mention in commit msg actually exists in repo.
        // * Detect area(sub-area) in the title that doesn't include area part (e.g., writing (bar) instead of foo(bar))
        // * Fix false positive raised by body-prose: "title\n\nParagraph begin. (Some text inside parens.)"

        {
            rules: {
                'body-prose': ({raw}: {raw:any}) => {
                    let offence = false;

                    let rawStr = convertAnyToString(raw, "raw").trim();
                    let lineBreakIndex = rawStr.indexOf('\n')
                    
                    if (lineBreakIndex >= 0){
                        // Extracting bodyStr from rawStr rather than using body directly is a 
                        // workaround for https://github.com/conventional-changelog/commitlint/issues/3412
                        let bodyStr = rawStr.substring(lineBreakIndex)

                        console.log("bodyStr:" + bodyStr)
                        for (let paragraph of bodyStr.trim().split('\n\n')){

                            // It's a paragraph that only consists of a block
                            if (/^```[^]*```$/.test(paragraph.trim())){
                                continue;
                            }
                            console.log("HERE2" + paragraph +"HERE3")
                            paragraph = removeAllCodeBlocks(paragraph).trim();

                            let startWithLowerCase = isLowerCase(paragraph[0])

                            let endsWithDotOrColon = paragraph[paragraph.length - 1] === '.' || paragraph[paragraph.length - 1] === ':';

                            let lines = paragraph.split(/\r?\n/)
                            
                            if (startWithLowerCase && 
                                !isValidUrl(lines[0])) {
                                offence = true;
                            }

                            if (!endsWithDotOrColon && 
                                !isValidUrl(lines[lines.length - 1]) && 
                                !isFooterNote(lines[lines.length - 1])) {

                                offence = true;
                            }

                            // if (startWithLowerCase || !endsWithDotOrColon){
                            //     let line = lines[0];
                                
                            //     // it's a URL
                            //     let isUrl = mightBeUrl(line);

                            //     let lineIsFooterNote = isFooterNote(line);

                            //     if ((!isUrl) && (!lineIsFooterNote)) {
                            //         offence = true;
                            //     }
                            // }
                        }
                                        
                    }

                    return [
                        !offence,
                        `Please begin a paragraph with uppercase letter and end it with a dot`
                    ];
                },

                'commit-hash-alone': ({raw}: {raw:any}) => {
                    let rawStr = convertAnyToString(raw, "raw");
                    console.log('rawStr:' + rawStr + 'EndOfRawStr')
                    let offence = false;

                    let urls = findUrls(rawStr)

                    let gitRepo = process.env['GITHUB_REPOSITORY'];
                    if (gitRepo !== undefined && urls !== null) {
                        for (let url of urls.entries()) {
                            let urlStr = url[1].toString()
                            if (isCommitUrl(urlStr) && urlStr.includes(gitRepo)) {
                                offence = true;
                                break;
                            }
                        }
                    }

                    return [
                        !offence,
                        `Please use the commit hash instead of the commit full URL`
                    ];
                },

                'empty-wip': ({header}: {header:any}) => {
                    let headerStr = convertAnyToString(header, "header");
                    let offence = headerStr.toLowerCase() === "wip";
                    return [
                        !offence,
                        `Please add a number or description after the WIP prefix`
                    ];
                },

                'header-max-length-with-suggestions': ({header}: {header:any}, _: any, maxLineLength:any) => {
                    let headerStr = convertAnyToString(header, "header");
                    let offence = false;

                    let message = `Please do not exceed ${maxLineLength} characters in title.`;
                    if (headerStr.length > maxLineLength) {
                        offence = true;
                        let numRecomendations = 0;
                        Object.entries(abbr).forEach(([key, value]) => {  
                            if (headerStr.includes(key.toString())){
                                if (numRecomendations === 0) {
                                    message = message + 'The following replacement(s) in your commit title are recommended:\n'
                                }

                                message = message + `"${key}" -> "${value}"\n`;             
                            }
                        })
                    }
                    
                    return [
                        !offence,
                        message
                    ];
                },

                'footer-notes-misplacement': ({body}: {body:any}) => {
                    let offence = false;

                    if (body !== null) {
                        let bodyStr = convertAnyToString(body, "body");

                        let seenBody = false;
                        let seenFooter = false;
                        let lines = bodyStr.split(/\r?\n/);
                        for (let line of lines) {
                            if (line.length === 0){
                                continue;
                            }
                            seenBody = seenBody || !isFooterNote(line);
                            seenFooter = seenFooter || isFooterNote(line);
                            if (seenFooter && !isFooterNote(line)) {
                                offence = true;
                                break;
                            }
                            
                        }
                    }
                    return [
                        !offence,
                        `Footer messages must be placed after body paragraphs, please move any message that starts with a "[]" or "Fixes" to the end of the commmit message.`
                    ]
                },

                'footer-references-existence': ({body}: {body:any}) => {
                    let offence = false;

                    if (body !== null) {
                        let bodyStr = convertAnyToString(body, "body");
                        let lines = bodyStr.split(/\r?\n/);
                        let bodyReferences = new Set();
                        let references = new Set();
                        for (let line of lines) {
                            let matches = line.match(/(?<=\[)([0-9]+)(?=\])/g);
                            if (matches === null) {
                                continue;
                            }
                            for (let match of matches){
                                if (isFooterReference(line)) {
                                    references.add(match);
                                }
                                else {
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
                    return [
                        !offence,
                        "All references in the body must be mentioned in the footer, and vice versa."
                    ]
                },

                'prefer-slash-over-backslash': ({header}: {header:any}) => {
                    let headerStr = convertAnyToString(header, "header");

                    let offence = false;

                    let colonIndex = headerStr.indexOf(":");
                    if (colonIndex >= 0){
                        let areaOrScope = headerStr.substring(0, colonIndex);
                        if (areaOrScope.includes('\\')){
                            offence = true;
                        }
                    }

                    return [
                        !offence,
                        `Please use slash instead of backslash in the area/scope/sub-area section of the title`
                    ];
                },

                'proper-issue-refs': ({raw}: {raw:any}) => {
                    let offence = false;

                    let rawStr = convertAnyToString(raw, "raw").trim();
                    let lineBreakIndex = rawStr.indexOf('\n')
                    
                    if (lineBreakIndex >= 0){
                        // Extracting bodyStr from rawStr rather than using body directly is a 
                        // workaround for https://github.com/conventional-changelog/commitlint/issues/3412
                        let bodyStr = rawStr.substring(lineBreakIndex)
                        bodyStr = removeAllCodeBlocks(bodyStr);
                        offence = includesHashtagRef(bodyStr);
                    }

                    return [
                        !offence,
                        `Please use full URLs instead of #XYZ refs.`
                    ];
                },


                'too-many-spaces': ({raw}: {raw:any}) => {
                    let rawStr = convertAnyToString(raw, "raw");
                    rawStr = removeAllCodeBlocks(rawStr);
                    let offence = (rawStr.match(`[^.]  `) !== null);

                    return [
                        !offence,
                        `Please watch out for too many whitespaces in the text`
                    ];
                },

                'type-space-after-colon': ({header}: {header:any}) => {
                    let headerStr = convertAnyToString(header, "header");

                    let colonFirstIndex = headerStr.indexOf(":");

                    let offence = false;
                    if ((colonFirstIndex > 0) && (headerStr.length > colonFirstIndex)) {
                        if (headerStr[colonFirstIndex + 1] != ' ') {
                            offence = true;
                        }
                    }

                    return [
                        !offence,
                        `Please place a space after the first colon character in your commit message title`
                    ];
                },

                'type-with-square-brackets': ({header}: {header:any}) => {
                    let headerStr = convertAnyToString(header, "header");

                    let offence = headerStr.match(`^\\[.*\\]`) !== null

                    return [
                        !offence,
                        `Please use "area/scope: subject" or "area(scope): subject" style instead of wrapping area/scope under square brackets in your commit message title`
                    ];
                },

                // NOTE: we use 'header' instead of 'subject' as a workaround to this bug: https://github.com/conventional-changelog/commitlint/issues/3404
                'subject-lowercase': ({header}: {header:any}) => {
                    let headerStr = convertAnyToString(header, "header");

                    let offence = false;
                    let colonFirstIndex = headerStr.indexOf(":");
                    if ((colonFirstIndex > 0) && (headerStr.length > colonFirstIndex)) {
                        let subject = headerStr.substring(colonFirstIndex + 1).trim();
                        if (subject != null && subject.length > 1) {
                            let firstWord = subject.trim().split(' ')[0];
                            offence = wordIsStartOfSentence(firstWord)
                        }
                    }

                    return [
                        !offence,
                        `Please use lowercase as the first letter for your subject, i.e. the text after your area/scope`
                    ];
                },

                'type-space-after-comma': ({header}: {header:any}) => {
                    let headerStr = convertAnyToString(header, "header");

                    let offence = false;
                    let colonIndex = headerStr.indexOf(":");
                    if (colonIndex >= 0){
                        let areaOrScope = headerStr.substring(0, colonIndex);
                        let commaIndex = (areaOrScope.indexOf(','));
                        while (commaIndex >= 0) {
                            if (areaOrScope[commaIndex + 1] === ' ') {
                                offence = true;
                            }
                            areaOrScope = areaOrScope.substring(commaIndex + 1);
                            commaIndex = (areaOrScope.indexOf(','));
                        }
                    }

                    return [
                        !offence,
                        `No need to use space after comma in the area/scope (so that commit title can be shorter)`
                    ];
                },

                'body-soft-max-line-length': ({body}: {body:any}) => {
                    let offence = false;

                    // does msg have a body?
                    if (body !== null) {
                        let bodyStr = convertAnyToString(body, "body");

                        let lines = bodyStr.split(/\r?\n/);
                        let inBigBlock = false;
                        for (let line of lines) {
                            if (isBigBlock(line)) {
                                inBigBlock = !inBigBlock;
                                continue;
                            }
                            if (inBigBlock) {
                                continue;
                            }
                            if (line.length > bodyMaxLineLength) {

                                // it's a URL
                                let isUrl = isValidUrl(line);

                                let lineIsFooterNote = isFooterNote(line);

                                if ((!isUrl) && (!lineIsFooterNote)) {
                                    offence = true;
                                    break;
                                }
                            }
                        }
                    }

                    // taken from https://stackoverflow.com/a/66433444/544947 and https://unix.stackexchange.com/a/25208/56844
                    function getUnixCommand(fmtOption: string){
                        return `git log --format=%B -n 1 $(git log -1 --pretty=format:"%h") | cat - > log.txt ; fmt -w 1111 -s log.txt > ulog.txt && fmt -w 64 -s ${fmtOption} ulog.txt > wlog.txt && git commit --amend -F wlog.txt`;
                    }

                    return [
                        !offence,
                        `Please do not exceed ${bodyMaxLineLength} characters in the lines of the commit message's body; we recommend this unix command (for editing the last commit message): \n` +
                        `For Linux users: ${getUnixCommand('-u')}\n` +
                        `For macOS users: ${getUnixCommand('')}`
                    ];
                },

                'trailing-whitespace': ({raw}: {raw:any}) => {
                    let rawStr = convertAnyToString(raw, "raw");

                    let offence = false;
                    let lines = rawStr.split(/\r?\n/);
                    let inBigBlock = false;
                    for (let line of lines) {
                        if (isBigBlock(line)) {
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
                        `Please watch out for leading or ending trailing whitespace`
                    ];
                },

                'type-space-before-paren': ({header}: {header:any}) => {
                    let headerStr = convertAnyToString(header, "header");

                    let offence = false;

                    let colonIndex = headerStr.indexOf(":");
                    if (colonIndex >= 0){
                        let areaOrScope = headerStr.substring(0, colonIndex);
                        let parenIndex = (areaOrScope.indexOf('('));
                        if (parenIndex >= 1){
                            if (headerStr[parenIndex - 1] === ' ') {
                                offence = true;
                            }
                        }    
                    }

                    return [
                        !offence,
                        `No need to use space before parentheses in the area/scope/sub-area section of the title`
                    ];
                },
            }
        }
    ]
};
