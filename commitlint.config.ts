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

function isBigBlock(line: string) {
    let bigBlockDelimiter = "```";
    return (line.length == bigBlockDelimiter.length) && (line.indexOf("```") == 0);
}

module.exports = {
    parserPreset: 'conventional-changelog-conventionalcommits',
    rules: {
        'body-prose': [RuleStatus.Error, 'always'],
        'body-leading-blank': [RuleStatus.Warning, 'always'],
        'body-soft-max-line-length': [RuleStatus.Error, 'always'],
        'footer-leading-blank': [RuleStatus.Warning, 'always'],
        'footer-max-line-length': [RuleStatus.Error, 'always', 150],
        'header-max-length': [RuleStatus.Error, 'always', 50],
        'subject-full-stop': [RuleStatus.Error, 'never', '.'],
        'type-empty': [RuleStatus.Warning, 'never'],
        'type-space-after-colon': [RuleStatus.Error, 'always'],
        'subject-lowercase': [RuleStatus.Error, 'always'],
        'type-space-after-comma': [RuleStatus.Error, 'always'],
        'trailing-whitespace': [RuleStatus.Error, 'always'],
        'prefer-slash-over-backslash': [RuleStatus.Error, 'always'],
    },
    plugins: [
        // TODO (ideas for more rules):
        // * Don't put space before parentheses (or slash) in area/scope.
        // * Better rule than body-max-line-length that ignores line if it starts with `[x] ` where x is a number.
        // * 'body-full-stop' which finds paragraphs in body without full-stop (which ignores lines in same way as suggested above).
        // * 'body-paragraph-uppercase' which finds paragraphs in body starting with lowercase.
        // * Detect if paragraphs in body have been cropped too shortly (less than 64 chars), and suggest same auto-wrap command that body-soft-max-line-length suggests, since it unwraps and wraps (both).
        // * Detect reverts which have not been elaborated.
        // * Detect WIP commits without a number.
        // * Reject #XYZ refs in favour for full URLs.
        // * If full URL for commit found, reject in favour for just the commit hash.
        // * Reject some stupid obvious words: change, update, modify (if first word after colon, error; otherwise warning).
        // * Think of how to reject this shitty commit message: https://github.com/nblockchain/NOnion/pull/34/commits/9ffcb373a1147ed1c729e8aca4ffd30467255594
        // * Title should not have dot at the end.
        // * Each body's paragraph should begin with uppercase and end with dot.
        // * Second line of commit msg should always be blank.
        // * Check for too many spaces (e.g. 2 spaces after colon)
        // * Detect area/scope wrapped under square brakets instead of "foo: bar" style.
        // * Workflow: detect if wip commit in a branch not named "wip/*" or whose name contains "squashed".
        // * Allow PascalCase word after colon in title (exception to subject-lowercase rule), e.g.: "End2End: TestFixtureSetup refactor"
        // * Detect if commit hash mention in commit msg actually exists in repo.
        // * Give replacement suggestions in rule that detects too long titles (e.g. and->&, config->cfg, ...)

        {
            rules: {
                'body-prose': ({body}: {body:any}) => {
                    let offence = false;

                    // does msg have a body?
                    if (body !== null) {
                        let bodyStr = convertAnyToString(body, "body");
                        
                        for (let par of bodyStr.trim().split('\n\n')){

                            // It's a paragraph that only consists of a block
                            if (/^```[^]*```$/.test(par.trim())){
                                continue;
                            }

                            par = par.replace(/```[^]*```/g, '').trim();

                            let firstIsUpperCase = par[0].toUpperCase() == par[0];
                            let firstIsLowerCase = par[0].toLowerCase() == par[0];
                            let startWithLowerCase = firstIsLowerCase && (!firstIsUpperCase);

                            let endsWithDotOrColon = par[par.length - 1] === '.' || par[par.length - 1] === ':';
                            
                            if (startWithLowerCase || !endsWithDotOrColon){
                                let line = par.split(/\r?\n/)[0];
                                
                                // it's a URL
                                let containsASpace = line.indexOf(" ") >= 0;
                                
                                // it's a footer reference, i.e. [1] someUrl://foo/bar/baz
                                let startsWithRef = (line[0] === "[" && line.indexOf("] ") > 0);

                                let startWithFixesSentence = (line.indexOf("Fixes ") == 0);

                                if (containsASpace && (!startsWithRef) && (!startWithFixesSentence)) {
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

                // NOTE: we use 'header' instead of 'subject' as a workaround to this bug: https://github.com/conventional-changelog/commitlint/issues/3404
                'subject-lowercase': ({header}: {header:any}) => {
                    let headerStr = convertAnyToString(header, "header");

                    let offence = false;
                    let colonFirstIndex = headerStr.indexOf(":");
                    if ((colonFirstIndex > 0) && (headerStr.length > colonFirstIndex)) {
                        let subject = headerStr.substring(colonFirstIndex + 1).trim();
                        if (subject != null && subject.length > 1) {
                            let firstIsUpperCase = subject[0].toUpperCase() == subject[0];
                            let firstIsLowerCase = subject[0].toLowerCase() == subject[0];
                            let secondIsUpperCase = subject[1].toUpperCase() == subject[1];
                            let secondIsLowerCase = subject[1].toLowerCase() == subject[1];

                            offence = firstIsUpperCase && (!firstIsLowerCase)
                                // to whitelist acronyms
                                && (!secondIsUpperCase) && secondIsLowerCase;
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
                                let containsASpace = line.indexOf(" ") >= 0;

                                // it's a footer reference, i.e. [1] someUrl://foo/bar/baz
                                let startsWithRef = (line[0] == "[" && line.indexOf("] ") > 0);

                                let startWithFixesSentence = (line.indexOf("Fixes ") == 0);

                                if (containsASpace && (!startsWithRef) && (!startWithFixesSentence)) {
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
            }
        }
    ]
};
