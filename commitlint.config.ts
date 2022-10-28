import { abbr } from "./abbreviations";
const execSync = require('child_process').execSync;

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

function findRemoteUrls() {
    // let output = ""
    // try {
    //     output = execSync('git remote -v', { encoding: 'utf-8' }).toString();
    // }
    // catch(e){
    //     console.log(e)
    // }
    
    // let remotes: Set<string> = new Set<string>();
    // let httpsUrls = output.match(/https:\/\/github.com\/([^.]*).git/g);
    // let sshUrls = output.match(/git@github.com:([^.]*).git/g);
    // if (httpsUrls !== null) {
    //     for (let match of httpsUrls) {
    //         let remoteRepo = match.substring(19, match.length-4);
    //         remotes.add(remoteRepo);
    //     }
    // }
    // if (sshUrls !== null) {
    //     for (let match of sshUrls) {
    //         let remoteRepo = match.substring(15, match.length-4);   
    //         remotes.add(remoteRepo);
    //     }
    // }
    let remotes: Set<string> = new Set<string>();
    // remotes.add()
    console.log(process.env)


    return remotes;
}

// function isRelatedToThisRepo(url: string) {
//     for (let remote of remotes) {
//         if (url.includes(remote)) {
//             return true;
//         }
//     }
//     return false
// }

function isValidUrl(url: string) {
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

function mightBeUrl(line: string) {
    assertLine(line);
    return line.indexOf(" ") < 0;
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
        'commit-hash-alone': [RuleStatus.Error, 'always'],
    },
    plugins: [
        // TODO (ideas for more rules):
        // * Better rule than body-max-line-length that ignores line if it starts with `[x] ` where x is a number.
        // * Detect if paragraphs in body have been cropped too shortly (less than 64 chars), and suggest same auto-wrap command that body-soft-max-line-length suggests, since it unwraps and wraps (both).
        // * Detect reverts which have not been elaborated.
        // * Reject #XYZ refs in favour for full URLs.
        // * If full URL for commit found, reject in favour for just the commit hash.
        // * Reject some stupid obvious words: change, update, modify (if first word after colon, error; otherwise warning).
        // * Think of how to reject this shitty commit message: https://github.com/nblockchain/NOnion/pull/34/commits/9ffcb373a1147ed1c729e8aca4ffd30467255594
        // * Title should not have dot at the end.
        // * Second line of commit msg should always be blank.
        // * Check for too many spaces (e.g. 2 spaces after colon)
        // * Workflow: detect if wip commit in a branch not named "wip/*" or whose name contains "squashed".
        // * Detect if commit hash mention in commit msg actually exists in repo.
        // * Detect area(sub-area) in the title that doesn't include area part (e.g., writing (bar) instead of foo(bar))
        // * Fix false positive raised by body-prose: "title\n\nParagraph begin. (Some text inside parens.)"

        {
            rules: {
                'body-prose': ({body}: {body:any}) => {
                    let offence = false;

                    // does msg have a body?
                    if (body !== null) {
                        let bodyStr = convertAnyToString(body, "body");

                        for (let paragraph of bodyStr.trim().split('\n\n')){

                            // It's a paragraph that only consists of a block
                            if (/^```[^]*```$/.test(paragraph.trim())){
                                continue;
                            }

                            paragraph = paragraph.replace(/```[^]*```/g, '').trim();

                            let startWithLowerCase = isLowerCase(paragraph[0])

                            let endsWithDotOrColon = paragraph[paragraph.length - 1] === '.' || paragraph[paragraph.length - 1] === ':';

                            if (startWithLowerCase || !endsWithDotOrColon){
                                let line = paragraph.split(/\r?\n/)[0];
                                
                                // it's a URL
                                let isUrl = mightBeUrl(line);

                                let lineIsFooterNote = isFooterNote(line);

                                if ((!isUrl) && (!lineIsFooterNote)) {
                                    offence = true;
                                }
                            }
                        }
                                        
                    }

                    return [
                        !offence,
                        `Please begin a paragraph with uppercase letter and end it with a dot`
                    ];
                },

                'commit-hash-alone': ({raw}: {raw:any}) => {
                    let rawStr = convertAnyToString(raw, "raw");
                    let offence = false;

                    let remotes = findRemoteUrls() 

                    // let urls = findUrls(rawStr)
                    // console.log('here1' + remotes)
                    // for (let remote of remotes) {
                    //     console.log(remote)
                    // }
                    // console.log('here2' + urls)
                    // if (urls !== null) {
                    //     for (let url of urls.entries()) {
                    //         let urlStr = url.toString()
                    //         if (isCommitUrl(urlStr) && isRelatedToThisRepo(urlStr)) {
                    //             offence = true;
                    //             break;
                    //         }
                    //     }
                    // }

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
                                let isUrl = mightBeUrl(line);

                                let lineIsFooterNote = isFooterNote(line);

                                if ((!isUrl) && (!lineIsFooterNote)) {
                                    offence = true;
                                    break;
                                }
                            }
                        }
                    }

                    // taken from https://stackoverflow.com/a/66433444/544947 and https://unix.stackexchange.com/a/25208/56844
                    let recommendedUnixCommand =
                        'git log --format=%B -n 1 $(git log -1 --pretty=format:"%h") | cat - > log.txt ; fmt -w 1111 -s log.txt > ulog.txt && fmt -w 64 -s ulog.txt > wlog.txt && git commit --amend -F wlog.txt';

                    return [
                        !offence,
                        `Please do not exceed ${bodyMaxLineLength} characters in the lines of the commit message's body; we recommend this unix command (for editing the last commit message): ${recommendedUnixCommand}`
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
