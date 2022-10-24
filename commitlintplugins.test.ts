const { spawnSync } = require('child_process');

function runCommitLintOnMsg(inputMsg: string) {
    return spawnSync('npx', ['commitlint', '--verbose'], { input: inputMsg });
}

test('body-prose1', () => {
    let commitMsgWithLowercaseBodyStart =
        "foo: this is only a title" + "\n\n" + "bla blah bla.";
    let bodyProse1 = runCommitLintOnMsg(commitMsgWithLowercaseBodyStart);
    expect(bodyProse1.status).not.toBe(0);
});


test('body-prose2', () => {
    let commitMsgWithNumbercaseBodyStart =
        "foo: this is only a title" + "\n\n" + "1234 bla blah bla.";
    let bodyProse2 = runCommitLintOnMsg(commitMsgWithNumbercaseBodyStart);
    expect(bodyProse2.status).toBe(0);
});


test('body-prose3', () => {
    let commitMsgWithUrl =
        "foo: this is only a title" + "\n\n" + "someUrl://blahblah.com";
    let bodyProse3 = runCommitLintOnMsg(commitMsgWithUrl);

    // because URLs can bypass the rule
    expect(bodyProse3.status).toBe(0);
});


test('body-prose4', () => {
    let commitMsgWithFootnoteUrl =
        "foo: this is only a title" + "\n\n" + "Bla blah[1] bla.\n\n[1] someUrl://blahblah.com";
    let bodyProse4 = runCommitLintOnMsg(commitMsgWithFootnoteUrl);

    // because URLs in footer can bypass the rule
    expect(bodyProse4.status).toBe(0);
});


test('body-prose5', () => {
    let commitMsgWithBugUrl =
        "foo: this is only a title" + "\n\n" + "Fixes someUrl://blahblah.com";
    let bodyProse5 = runCommitLintOnMsg(commitMsgWithBugUrl);

    // because URLs in "Fixes <URL>" sentence can bypass the rule
    expect(bodyProse5.status).toBe(0);
});


test('body-prose6', () => {
    let commitMsgWithBlock =
        "foo: this is only a title\n\nBar baz.\n\n```\nif (foo) { bar(); }\n```";
    let bodyProse6 = runCommitLintOnMsg(commitMsgWithBlock);

    // because ```blocks surrounded like this``` can bypass the rule
    expect(bodyProse6.status).toBe(0);
});


test('body-prose7', () => {
    let commitMsgWithParagraphEndingWithColon =
        "foo: this is only a title" + "\n\n" + "Bar baz:\n\nBlah blah.";
    let bodyProse7 = runCommitLintOnMsg(commitMsgWithParagraphEndingWithColon);

    // because paragraphs can end with a colon
    expect(bodyProse7.status).toBe(0);
});


test('body-prose8', () => {
    let commitMsgWithCoAuthoredByTagThatIsObviouslyNotAParagraph =
        "foo: this is only a title" + "\n\n" + "Co-authored-by: Jon Doe <jondoe@example.com>";
    let bodyProse8 = runCommitLintOnMsg(commitMsgWithCoAuthoredByTagThatIsObviouslyNotAParagraph);

    // because Co-authored-by tags don't end with a dot
    expect(bodyProse8.status).toBe(0);
});


test('body-max-line-length1', () => {
    let tenChars = "1234 67890";
    let sixtyChars = tenChars + tenChars + tenChars + tenChars + tenChars + tenChars;
    let commitMsgWithOnlySixtyFourCharsInBody =
        "foo: this is only a title" + "\n\n" + sixtyChars + "123.";
    let bodyMaxLineLength1 = runCommitLintOnMsg(commitMsgWithOnlySixtyFourCharsInBody);
    expect(bodyMaxLineLength1.status).toBe(0);
});


test('body-max-line-length2', () => {
    let tenChars = "1234 67890";
    let sixtyChars = tenChars + tenChars + tenChars + tenChars + tenChars + tenChars;
    let commitMsgWithOnlySixtyFiveCharsInBody =
        "foo: this is only a title" + "\n\n" + sixtyChars + "1234.";
    let bodyMaxLineLength2 = runCommitLintOnMsg(commitMsgWithOnlySixtyFiveCharsInBody);
    expect(bodyMaxLineLength2.status).not.toBe(0);
});


test('body-max-line-length3', () => {
    let tenDigits = "1234567890";
    let seventyChars = tenDigits + tenDigits + tenDigits + tenDigits + tenDigits + tenDigits + tenDigits;
    let commitMsgWithUrlThatExceedsBodyMaxLineLength =
        "foo: this is only a title" + "\n\n" + "someUrl://" + seventyChars;
    let bodyMaxLineLength3 = runCommitLintOnMsg(commitMsgWithUrlThatExceedsBodyMaxLineLength);

    // because URLs can bypass the limit
    expect(bodyMaxLineLength3.status).toBe(0);
});


test('body-max-line-length4', () => {
    let tenDigits = "1234567890";
    let seventyChars = tenDigits + tenDigits + tenDigits + tenDigits + tenDigits + tenDigits + tenDigits;
    let commitMsgWithUrlThatExceedsBodyMaxLineLength =
        "foo: this is only a title" + "\n\n" + "Bla blah[1] bla.\n\n[1] someUrl://" + seventyChars;
    let bodyMaxLineLength4 = runCommitLintOnMsg(commitMsgWithUrlThatExceedsBodyMaxLineLength);

    // because URLs in footer can bypass the limit
    expect(bodyMaxLineLength4.status).toBe(0);
});


test('body-max-line-length5', () => {
    let tenDigits = "1234567890";
    let seventyChars = tenDigits + tenDigits + tenDigits + tenDigits + tenDigits + tenDigits + tenDigits;
    let commitMsgWithUrlThatExceedsBodyMaxLineLength =
        "foo: this is only a title" + "\n\n" + "Fixes someUrl://" + seventyChars;
    let bodyMaxLineLength5 = runCommitLintOnMsg(commitMsgWithUrlThatExceedsBodyMaxLineLength);

    // because URLs in "Fixes <URL>" sentence can bypass the limit
    expect(bodyMaxLineLength5.status).toBe(0);
});


test('body-max-line-length6', () => {
    let tenChars = "1234 67890";
    let seventyChars = tenChars + tenChars + tenChars + tenChars + tenChars + tenChars + tenChars;
    let commitMsgWithUrlThatExceedsBodyMaxLineLength =
        "foo: this is only a title" + "\n\n" + "Bar baz.\n```\n" + seventyChars + "\n```";
    let bodyMaxLineLength6 = runCommitLintOnMsg(commitMsgWithUrlThatExceedsBodyMaxLineLength);

    // because ```blocks surrounded like this``` can bypass the limit
    expect(bodyMaxLineLength6.status).toBe(0);
});


test('body-max-line-length7', () => {
    let tenChars = "1234567890";
    let seventyChars = tenChars + tenChars + tenChars + tenChars + tenChars + tenChars + tenChars;
    let commitMsgWithCoAuthoredByTagThatExceedsBodyMaxLineLength =
        "foo: this is only a title" + "\n\n" + "Co-authored-by: Jon Doe <" + seventyChars + "@example.com>";
    let bodyMaxLineLength7 = runCommitLintOnMsg(commitMsgWithCoAuthoredByTagThatExceedsBodyMaxLineLength);

    // because Co-authored-by tags can bypass the limit
    expect(bodyMaxLineLength7.status).toBe(0);
});


test('empty-wip-1', () => {
    let commitMsgWithEpmtyWIP = "WIP";
    let emptyWIP1 = runCommitLintOnMsg(commitMsgWithEpmtyWIP);
    expect(emptyWIP1.status).not.toBe(0);
});


test('empty-wip-2', () => {
    let commitMsgWithDescriptionAfterWIP = "WIP: bla bla blah";
    let emptyWIP2 = runCommitLintOnMsg(commitMsgWithDescriptionAfterWIP);
    expect(emptyWIP2.status).toBe(0);
});


test('empty-wip-3', () => {
    let commitMsgWithNumberAfterWIP = "WIP1";
    let emptyWIP3 = runCommitLintOnMsg(commitMsgWithNumberAfterWIP);
    expect(emptyWIP3.status).toBe(0);
});


test('footer-notes-misplacement-1', () => {
    let commitMsgWithRightFooter = "foo: this is only a title" 
        + "\n\n"+ "Bla bla blah[1]." 
        + "\n\n" + "Fixes https://some/issue" 
        + "\n\n" + "[1] http://foo.bar/baz";
    let footerNotesMisplacement1 = runCommitLintOnMsg(commitMsgWithRightFooter);
    expect(footerNotesMisplacement1.status).toBe(0);
})


test('footer-notes-misplacement-2', () => {
    let commitMsgWithWrongFooter = "foo: this is only a title" 
        + "\n\n" + "Fixes https://some/issue" 
        + "\n\n"+ "Bla bla blah[1]." 
        + "\n\n" + "[1] http://foo.bar/baz";
    let footerNotesMisplacement2 = runCommitLintOnMsg(commitMsgWithWrongFooter);
    expect(footerNotesMisplacement2.status).not.toBe(0);
})


test('footer-notes-misplacement-3', () => {
    let commitMsgWithWrongFooter = "foo: this is only a title" 
        + "\n\n"+ "Bla bla blah[1]." 
        + "\n\n" + "[1] http://foo.bar/baz"
        + "\n\n" + "Some other bla bla blah."
        + "\n\n" + "Fixes https://some/issue";
    let footerNotesMisplacement3 = runCommitLintOnMsg(commitMsgWithWrongFooter);
    expect(footerNotesMisplacement3.status).not.toBe(0);
})


test('prefer-slash-over-backslash1', () => {
    let commitMsgWithBackslash = "foo\\bar: bla bla bla";
    let preferSlashOverBackslash1 = runCommitLintOnMsg(commitMsgWithBackslash);
    expect(preferSlashOverBackslash1.status).not.toBe(0);
});


test('prefer-slash-over-backslash2', () => {
    let commitMsgWithSlash = "foo/bar: bla bla bla";
    let preferSlashOverBackslash2 = runCommitLintOnMsg(commitMsgWithSlash);
    expect(preferSlashOverBackslash2.status).toBe(0);

});


test('header-max-length-with-suggestions1', () => {
    let commitMsgWithThatExceedsHeaderMaxLength =
        "foo: this is only a title with a configuration in it that exceeds header max length";
    let headerMaxLength1 = runCommitLintOnMsg(commitMsgWithThatExceedsHeaderMaxLength);
    let expected_message = `"configuration" -> "config"`
    // console.log('HERE ==========>' + replacementSuggestion1.stdout)
    expect(headerMaxLength1.status).not.toBe(0);
    expect((headerMaxLength1.stdout + '').includes(expected_message)).toEqual(true)
});


test('header-max-length-with-suggestions2', () => {
    let commitMsgWithThatExceedsHeaderMaxLength =
        "foo: this is only a title with a 1 second in it that exceeds header max length";
    let headerMaxLength2 = runCommitLintOnMsg(commitMsgWithThatExceedsHeaderMaxLength);
    let expected_message = `"1 second" -> "1sec"`
    // console.log('HERE ==========>' + replacementSuggestion1.stdout)
    expect(headerMaxLength2.status).not.toBe(0);
    expect((headerMaxLength2.stdout + '').includes(expected_message)).toEqual(true)
});


test('header-max-length-with-suggestions3', () => {
    let commitMsgWithOnlyTwentySixCharsInTitle =
        "foo: this is only a title";
    let headerMaxLength3 = runCommitLintOnMsg(commitMsgWithOnlyTwentySixCharsInTitle);
    expect(headerMaxLength3.status).toBe(0);
});


test('header-max-length-with-suggestions4', () => {
    let tenChars = '1234 12345'
    let commitMsgWithOnlyFiftyCharsInTitle =
        "foo: 12345" + tenChars + tenChars + tenChars + tenChars;
    let headerMaxLength4 = runCommitLintOnMsg(commitMsgWithOnlyFiftyCharsInTitle);
    expect(headerMaxLength4.status).toBe(0);
});


test('reject-hashtag-refs1', () => {
    let commitMsgWithHashtagRef = "foo: blah blah" + '\n\n' + "Blah blah #123";
    let rejectHashtagRefs1 = runCommitLintOnMsg(commitMsgWithHashtagRef);
    expect(rejectHashtagRefs1.status).not.toBe(0);
});


test('reject-hashtag-refs2', () => {
    let commitMsgWithFullUrl = "foo: blah blah" + '\n\n' + 'Fixes someUrl://blah.blah/158';
    let rejectHashtagRefs1 = runCommitLintOnMsg(commitMsgWithFullUrl);
    expect(rejectHashtagRefs1.status).toBe(0);
});


test('reject-hashtag-refs3', () => {
    let commitMsgWithHashtagRefInBlock =
        "foo: this is only a title" + "\n\n" + "Bar baz:\n\n```\ntype Foo = string #123\n```";
    let trailingWhitespace6 = runCommitLintOnMsg(commitMsgWithHashtagRefInBlock);
    expect(trailingWhitespace6.status).toBe(0);
});


test('subject-lowercase1', () => {
    let commitMsgWithUppercaseAfterColon = "foo: Bar baz";
    let subjectLowerCase1 = runCommitLintOnMsg(commitMsgWithUppercaseAfterColon);
    expect(subjectLowerCase1.status).not.toBe(0);
});


test('subject-lowercase2', () => {
    let commitMsgWithLowercaseAfterColon = "foo: bar baz";
    let subjectLowerCase2 = runCommitLintOnMsg(commitMsgWithLowercaseAfterColon);
    expect(subjectLowerCase2.status).toBe(0);
});


test('subject-lowercase3', () => {
    let commitMsgWithAcronymAfterColon = "foo: BAR baz";
    let subjectLowerCase3 = runCommitLintOnMsg(commitMsgWithAcronymAfterColon);
    expect(subjectLowerCase3.status).toBe(0);
});


test('subject-lowercase4', () => {
    let commitMsgWithNonAlphanumericAfterColon = "foo: 3 tests added";
    let subjectLowerCase4 = runCommitLintOnMsg(commitMsgWithNonAlphanumericAfterColon);
    expect(subjectLowerCase4.status).toBe(0);
});


test('subject-lowercase5', () => {
    let commitMsgWithRareCharInArea1 = "foo.bar: Baz";
    let subjectLowerCase5 = runCommitLintOnMsg(commitMsgWithRareCharInArea1);
    expect(subjectLowerCase5.status).not.toBe(0);
});


test('subject-lowercase6', () => {
    let commitMsgWithRareCharInArea2 = "foo-bar: Baz";
    let subjectLowerCase6 = runCommitLintOnMsg(commitMsgWithRareCharInArea2);
    expect(subjectLowerCase6.status).not.toBe(0);
});


test('subject-lowercase7', () => {
    let commitMsgWithRareCharInArea3 = "foo,bar: Baz";
    let subjectLowerCase7 = runCommitLintOnMsg(commitMsgWithRareCharInArea3);
    expect(subjectLowerCase7.status).not.toBe(0);
});


test('subject-lowercase8', () => {
    let commitMsgWithPascalCaseAfterColon = "End2End: TestFixtureSetup refactor";
    let subjectLowerCase8 = runCommitLintOnMsg(commitMsgWithPascalCaseAfterColon);
    expect(subjectLowerCase8.status).toBe(0);
});


test('subject-lowercase9', () => {
    let commitMsgWithCamelCaseAfterColon = "End2End: testFixtureSetup refactor";
    let subjectLowerCase9 = runCommitLintOnMsg(commitMsgWithCamelCaseAfterColon);
    // console.log("=============>" + subjectLowerCase9.stdout);
    expect(subjectLowerCase9.status).toBe(0);
});


test('subject-lowercase10', () => {
    let commitMsgWithNumber = "foo: A1 bar";
    let subjectLowerCase10 = runCommitLintOnMsg(commitMsgWithNumber);
    // console.log("=============>" + subjectLowerCase10.stdout);
    expect(subjectLowerCase10.status).toBe(0);
});


test('trailing-whitespace1', () => {
    let commitMsgWithNoTrailingWhiteSpace =
        "foo: this is only a title" + "\n\n" + "Bla blah bla.";
    let trailingWhitespace1 = runCommitLintOnMsg(commitMsgWithNoTrailingWhiteSpace);
    expect(trailingWhitespace1.status).toBe(0);
});


test('trailing-whitespace2', () => {
    let commitMsgWithTrailingWhiteSpaceInTitleEnd =
        "foo: title " + "\n\n" + "Bla blah bla.";
    let trailingWhitespace2 = runCommitLintOnMsg(commitMsgWithTrailingWhiteSpaceInTitleEnd);
    expect(trailingWhitespace2.status).not.toBe(0);
});


test('trailing-whitespace3', () => {
    let commitMsgWithTrailingWhiteSpaceInTitleStart =
        " foo: title" + "\n\n" + "Bla blah bla.";
    let trailingWhitespace3 = runCommitLintOnMsg(commitMsgWithTrailingWhiteSpaceInTitleStart);
    expect(trailingWhitespace3.status).not.toBe(0);
});


test('trailing-whitespace4', () => {
    let commitMsgWithTrailingWhiteSpaceInBodyStart =
        "foo: title" + "\n\n" + " Bla blah bla.";
    let trailingWhitespace4 = runCommitLintOnMsg(commitMsgWithTrailingWhiteSpaceInBodyStart);
    expect(trailingWhitespace4.status).not.toBe(0);
});


test('trailing-whitespace5', () => {
    let commitMsgWithTrailingWhiteSpaceInBodyEnd =
        "foo: title" + "\n\n" + "Bla blah bla. ";
    let trailingWhitespace5 = runCommitLintOnMsg(commitMsgWithTrailingWhiteSpaceInBodyEnd);
    expect(trailingWhitespace5.status).not.toBe(0);
});


test('trailing-whitespace6', () => {
    let commitMsgWithTrailingWhiteSpaceInCodeBlock =
        "foo: this is only a title" + "\n\n" + "Bar baz:\n\n```\ntype Foo =\n    string\n```";
    let trailingWhitespace6 = runCommitLintOnMsg(commitMsgWithTrailingWhiteSpaceInCodeBlock);
    //console.log("=============>" + trailingWhitespace6.stdout);
    expect(trailingWhitespace6.status).toBe(0);
});


test('type-space-after-colon1', () => {
    let commitMsgWithNoSpace = 'foo:bar';
    let typeSpaceAfterColon1 = runCommitLintOnMsg(commitMsgWithNoSpace);
    expect(typeSpaceAfterColon1.status).not.toBe(0);
});


test('type-space-after-colon2', () => {
    let commitMsgWithSpace = 'foo: bar';
    let typeSpaceAfterColon2 = runCommitLintOnMsg(commitMsgWithSpace);
    expect(typeSpaceAfterColon2.status).toBe(0);
});


test('type-space-after-colon3', () => {
    let commitMsgWithNoSpaceBeforeColonButAtTheEnd = 'foo: a tale of bar:baz';
    let typeSpaceAfterColon3 = runCommitLintOnMsg(commitMsgWithNoSpaceBeforeColonButAtTheEnd);
    expect(typeSpaceAfterColon3.status).toBe(0);
});


test('type-space-after-comma1', () => {
    let commitMsgWithSpaceAfterCommaInType = "foo, bar: bla bla blah";
    let typeSpaceAfterComma1 = runCommitLintOnMsg(commitMsgWithSpaceAfterCommaInType);
    expect(typeSpaceAfterComma1.status).not.toBe(0);
});


test('type-space-after-comma2', () => {
    let commitMsgWithNoSpaceAfterCommaInType = "foo,bar: bla bla blah";
    let typeSpaceAfterComma2 = runCommitLintOnMsg(commitMsgWithNoSpaceAfterCommaInType);
    expect(typeSpaceAfterComma2.status).toBe(0);
});


test('type-space-before-paren1', () => {
    let commitMsgWithNoSpaceBeforeParen = "foo (bar): bla bla bla";
    let typeSpaceBeforeParen1 = runCommitLintOnMsg(commitMsgWithNoSpaceBeforeParen);
    expect(typeSpaceBeforeParen1.status).not.toBe(0);
});


test('type-space-before-paren2', () => {
    let commitMsgWithNoSpaceBeforeParen = "foo(bar): bla bla bla";
    let typeSpaceBeforeParen2 = runCommitLintOnMsg(commitMsgWithNoSpaceBeforeParen);
    expect(typeSpaceBeforeParen2.status).toBe(0);
});


test('type-space-before-paren3', () => {
    let commitMsgWithNoSpaceBeforeParen = "(bar): bla bla bla";
    let typeSpaceBeforeParen3 = runCommitLintOnMsg(commitMsgWithNoSpaceBeforeParen);
    expect(typeSpaceBeforeParen3.status).toBe(0);
});


test('type-with-square-brackets1', () => {
    let commitMsgWithSquareBrackets = "[foo] this is a title";
    let typeWithSquareBrackets1 = runCommitLintOnMsg(commitMsgWithSquareBrackets);
    expect(typeWithSquareBrackets1.status).not.toBe(0);
});


test('type-with-square-brackets2', () => {
    let commitMsgWithoutSquareBrackets = "foo: this is a title";
    let typeWithSquareBrackets2 = runCommitLintOnMsg(commitMsgWithoutSquareBrackets);
    expect(typeWithSquareBrackets2.status).toBe(0);
});
