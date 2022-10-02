module.exports = {
    parserPreset: 'conventional-changelog-conventionalcommits',
    rules: {
        'body-leading-blank': [1, 'always'],
// disable this one until we find a way for URLs to be allowed:
//      'body-max-line-length': [2, 'always', 64],
        'footer-leading-blank': [1, 'always'],
        'footer-max-line-length': [2, 'always', 150],
        'header-max-length': [2, 'always', 50],
        'subject-full-stop': [2, 'never', '.'],
        'type-empty': [1, 'never'],
        'type-space-after-colon': [2, 'always'],
        'subject-lowercase': [2, 'always'],
    },
    plugins: [
        // TODO (ideas for more rules):
        // * Don't put space after comma in area/scope (type).
        // * Don't put space before parentheses or slash in area/scope.
        // * Better rule than body-max-line-length that ignores line if it starts with `[x] ` where x is a number.
        // * 'body-full-stop' which finds paragraphs in body without full-stop (which ignores lines in same way as suggested above).
        // * 'body-paragraph-uppercase' which finds paragraphs in body starting with lowercase.
        // * Detect if paragraphs in body have been cropped too shortly (less than 64 chars). -> maybe only a warning
        // * Detect trailing spaces.
        // * Detect reverts which have not been elaborated.
        // * Detect WIP commits without a number.
        // * Reject #XYZ refs in favour for full URLs.
        // * If full URL for commit found, reject in favour for just the commit hash.
        // * Reject some stupid obvious words: change, update, modify (if first word after colon, error; otherwise warning).
        // * Think of how to reject this shitty commit message: https://github.com/nblockchain/NOnion/pull/34/commits/9ffcb373a1147ed1c729e8aca4ffd30467255594
        // * Title should not have dot at the end.
        // * Each body's paragraph should begin with uppercase and end with dot.
        // * Second line of commit msg should always be blank.
        // * Check for trailing spaces (at the start and end of each line) or too many spaces (e.g. 2 spaces after colon)

        {
            rules: {
                'type-space-after-colon': ({header}: {header:any}) => {
                    if (header === null || header === undefined) {
                        // otherwise, String(null) might give us the stupid string "null"
                        throw new Error('Unexpected header===null or header===undefined happened');
                    }
                    // to convert from 'any' type
                    let headerStr = String(header);

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
                    if (header === null || header === undefined) {
                        // otherwise, String(null) might give us the stupid string "null"
                        throw new Error('Unexpected header===null or header===undefined happened');
                    }
                    // to convert from 'any' type
                    let headerStr = String(header);

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
                }
            }
        }
    ]
};
