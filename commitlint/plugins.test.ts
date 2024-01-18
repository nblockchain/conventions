import { runCommitLintOnMsg } from "./testHelpers";

test("body-prose1", () => {
    let commitMsgWithLowercaseBodyStart = `foo: this is only a title

bla blah bla.`;

    let bodyProse1 = runCommitLintOnMsg(commitMsgWithLowercaseBodyStart);
    expect(bodyProse1.status).not.toBe(0);
});

test("body-prose2", () => {
    let commitMsgWithNumbercaseBodyStart = `foo: this is only a title

1234 bla blah bla.`;

    let bodyProse2 = runCommitLintOnMsg(commitMsgWithNumbercaseBodyStart);
    expect(bodyProse2.status).toBe(0);
});

test("body-prose3", () => {
    let commitMsgWithUrl = `foo: this is only a title

someUrl://blahblah.com`;

    let bodyProse3 = runCommitLintOnMsg(commitMsgWithUrl);

    // because URLs can bypass the rule
    expect(bodyProse3.status).toBe(0);
});

test("body-prose4", () => {
    let commitMsgWithFootnoteUrl = `foo: this is only a title

Bla blah[1] bla.

[1] someUrl://blahblah.com`;

    let bodyProse4 = runCommitLintOnMsg(commitMsgWithFootnoteUrl);

    // because URLs in footer can bypass the rule
    expect(bodyProse4.status).toBe(0);
});

test("body-prose5", () => {
    let commitMsgWithBugUrl = `foo: this is only a title

Fixes someUrl://blahblah.com`;

    let bodyProse5 = runCommitLintOnMsg(commitMsgWithBugUrl);

    // because URLs in "Fixes <URL>" sentence can bypass the rule
    expect(bodyProse5.status).toBe(0);
});

test("body-prose6", () => {
    let commitMsgWithBlock =
        "foo: this is only a title\n\nBar baz.\n\n```\nif (foo) { bar(); }\n```";
    let bodyProse6 = runCommitLintOnMsg(commitMsgWithBlock);

    // because ```blocks surrounded like this``` can bypass the rule
    expect(bodyProse6.status).toBe(0);
});

test("body-prose7", () => {
    let commitMsgWithParagraphEndingWithColon = `foo: this is only a title

Bar baz:

Blah blah.`;

    let bodyProse7 = runCommitLintOnMsg(commitMsgWithParagraphEndingWithColon);

    // because paragraphs can end with a colon
    expect(bodyProse7.status).toBe(0);
});

test("body-prose8", () => {
    let commitMsgWithCoAuthoredByTagThatIsObviouslyNotAParagraph = `foo: this is only a title

Co-authored-by: Jon Doe <jondoe@example.com>`;

    let bodyProse8 = runCommitLintOnMsg(
        commitMsgWithCoAuthoredByTagThatIsObviouslyNotAParagraph
    );

    // because Co-authored-by tags don't end with a dot
    expect(bodyProse8.status).toBe(0);
});

test("body-prose9", () => {
    let commitMsgWithCommitUrlAtTheEndOfBodyParagraph = `foo: this is only a title

Foo bar:
https://github.com/username/repo/commit/1234567891234567891234567891234567891234`;

    let bodyProse9 = runCommitLintOnMsg(
        commitMsgWithCommitUrlAtTheEndOfBodyParagraph
    );

    expect(bodyProse9.status).toBe(0);
});

test("body-prose10", () => {
    let commitMsgWithLargeBody =
        "Network,TorHandshakes: handle handshake fail\n\n" +
        "```\nThe active test run was aborted. System.Exception: Key handshake failed!\n\n" +
        "at System.Threading.ThreadPoolWorkQueue.Dispatch()\n```";

    let bodyProse10 = runCommitLintOnMsg(commitMsgWithLargeBody);
    expect(bodyProse10.status).toBe(0);
});

test("body-prose11", () => {
    let commitMsgWithLargeBody =
        "Backend/Ether: catch/retry new -32002 err code\n\n" +
        "CI on master branch caught this[1]:\n\n" +
        "```\nUnhandled Exception\n```\n\n" +
        "[1] https://github.com/nblockchain/geewallet/actions/runs/3507005645/jobs/5874411684";

    let bodyProse11 = runCommitLintOnMsg(commitMsgWithLargeBody);
    expect(bodyProse11.status).toBe(0);
});

test("body-prose12", () => {
    let commitMsgWithBugUrl = `foo: this is only a title

Closes someUrl://blahblah.com`;

    let bodyProse12 = runCommitLintOnMsg(commitMsgWithBugUrl);

    // because URLs in "Closes <URL>" sentence can bypass the rule, similar to body-prose5
    expect(bodyProse12.status).toBe(0);
});

test("body-prose13", () => {
    let commitMsgWithParagraphEndingWithQuestionMark = `foo: this is only a title

Increase verbosity, because why not?

Blah blah.`;

    let bodyProse13 = runCommitLintOnMsg(
        commitMsgWithParagraphEndingWithQuestionMark
    );

    // because paragraphs can end with a question mark
    expect(bodyProse13.status).toBe(0);
});

test("body-prose14", () => {
    let commitMsgWithParagraphEndingWithExclamationMark = `foo: this is only a title

Increase verbosity, because why not!

Blah blah.`;

    let bodyProse14 = runCommitLintOnMsg(
        commitMsgWithParagraphEndingWithExclamationMark
    );

    // because paragraphs can end with a question mark
    expect(bodyProse14.status).toBe(0);
});

test("body-prose15", () => {
    let commitMsgWithParagraphEndingInParentheses = `foo: this is only a title

Paragraph begin. (Some text inside parens.)

Paragraph begin. (Some text inside parens?)

Paragraph begin. (Some text inside parens!)

Paragraph begin. Now a smiley! :)

Blah blah.`;

    let bodyProse15 = runCommitLintOnMsg(
        commitMsgWithParagraphEndingInParentheses
    );

    // because paragraphs can end with a question mark
    expect(bodyProse15.status).toBe(0);
});

test("body-prose16", () => {
    let commitMsgWithUrlAtTheEndOfParagraph = `Frontend.XF.Android: switch to SDK-style

Strangely enough when opening the new gwallet.android.sln in
VS4Mac, it asks the user to install the wasm-tools-net6.0
workload, even though CI works without it.

Original PR: https://github.com/nblockchain/geewallet/pull/190`;

    let bodyProse16 = runCommitLintOnMsg(commitMsgWithUrlAtTheEndOfParagraph);
    expect(bodyProse16.status).toBe(0);
});

test("body-prose17", () => {
    let commitMsgWithWindowsEOL =
        "title: this is only title\r\n\r\n" +
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit\r\n" +
        "lorem ipsum dolor sit amet, consectetur porttitor jidga\r\n" +
        "nam sed porttitor turpis, vitae erat curae.";
    let bodyProse17 = runCommitLintOnMsg(commitMsgWithWindowsEOL);
    expect(bodyProse17.status).toBe(0);
});

test("body-max-line-length1", () => {
    let tenChars = "1234 67890";
    let sixtyChars =
        tenChars + tenChars + tenChars + tenChars + tenChars + tenChars;
    let commitMsgWithOnlySixtyFourCharsInBody =
        "foo: this is only a title" + "\n\n" + sixtyChars + "123.";
    let bodyMaxLineLength1 = runCommitLintOnMsg(
        commitMsgWithOnlySixtyFourCharsInBody
    );
    expect(bodyMaxLineLength1.status).toBe(0);
});

test("body-max-line-length2", () => {
    let tenChars = "1234 67890";
    let sixtyChars =
        tenChars + tenChars + tenChars + tenChars + tenChars + tenChars;
    let commitMsgWithOnlySixtyFiveCharsInBody =
        "foo: this is only a title" + "\n\n" + sixtyChars + "1234.";
    let bodyMaxLineLength2 = runCommitLintOnMsg(
        commitMsgWithOnlySixtyFiveCharsInBody
    );
    expect(bodyMaxLineLength2.status).not.toBe(0);
});

test("body-max-line-length3", () => {
    let tenDigits = "1234567890";
    let seventyChars =
        tenDigits +
        tenDigits +
        tenDigits +
        tenDigits +
        tenDigits +
        tenDigits +
        tenDigits;
    let commitMsgWithUrlThatExceedsBodyMaxLineLength =
        "foo: this is only a title" + "\n\n" + "someUrl://" + seventyChars;
    let bodyMaxLineLength3 = runCommitLintOnMsg(
        commitMsgWithUrlThatExceedsBodyMaxLineLength
    );

    // because URLs can bypass the limit
    expect(bodyMaxLineLength3.status).toBe(0);
});

test("body-max-line-length4", () => {
    let tenDigits = "1234567890";
    let seventyChars =
        tenDigits +
        tenDigits +
        tenDigits +
        tenDigits +
        tenDigits +
        tenDigits +
        tenDigits;
    let commitMsgWithUrlThatExceedsBodyMaxLineLength =
        "foo: this is only a title" +
        "\n\n" +
        "Bla blah[1] bla.\n\n[1] someUrl://" +
        seventyChars;
    let bodyMaxLineLength4 = runCommitLintOnMsg(
        commitMsgWithUrlThatExceedsBodyMaxLineLength
    );

    // because URLs in footer can bypass the limit
    expect(bodyMaxLineLength4.status).toBe(0);
});

test("body-max-line-length5", () => {
    let tenDigits = "1234567890";
    let seventyChars =
        tenDigits +
        tenDigits +
        tenDigits +
        tenDigits +
        tenDigits +
        tenDigits +
        tenDigits;
    let commitMsgWithUrlThatExceedsBodyMaxLineLength =
        "foo: this is only a title" +
        "\n\n" +
        "Fixes someUrl://" +
        seventyChars;
    let bodyMaxLineLength5 = runCommitLintOnMsg(
        commitMsgWithUrlThatExceedsBodyMaxLineLength
    );

    // because URLs in "Fixes <URL>" sentence can bypass the limit
    expect(bodyMaxLineLength5.status).toBe(0);
});

test("body-max-line-length6", () => {
    let tenChars = "1234 67890";
    let seventyChars =
        tenChars +
        tenChars +
        tenChars +
        tenChars +
        tenChars +
        tenChars +
        tenChars;
    let commitMsgWithUrlThatExceedsBodyMaxLineLength =
        "foo: this is only a title" +
        "\n\n" +
        "Bar baz.\n```\n" +
        seventyChars +
        "\n```";
    let bodyMaxLineLength6 = runCommitLintOnMsg(
        commitMsgWithUrlThatExceedsBodyMaxLineLength
    );

    // because ```blocks surrounded like this``` can bypass the limit
    expect(bodyMaxLineLength6.status).toBe(0);
});

test("body-max-line-length7", () => {
    let tenChars = "1234567890";
    let seventyChars =
        tenChars +
        tenChars +
        tenChars +
        tenChars +
        tenChars +
        tenChars +
        tenChars;
    let commitMsgWithCoAuthoredByTagThatExceedsBodyMaxLineLength =
        "foo: this is only a title" +
        "\n\n" +
        "Co-authored-by: Jon Doe <" +
        seventyChars +
        "@example.com>";
    let bodyMaxLineLength7 = runCommitLintOnMsg(
        commitMsgWithCoAuthoredByTagThatExceedsBodyMaxLineLength
    );

    // because Co-authored-by tags can bypass the limit
    expect(bodyMaxLineLength7.status).toBe(0);
});

test("body-max-line-length8", () => {
    let commitMsgWithLargeBody = `Network,TorHandshakes: handle handshake fail

--- Line between dashes ---
A very long line. A very long line. A very long line. A very long line. A very long line. A very long line.`;

    let bodyMaxLineLength8 = runCommitLintOnMsg(commitMsgWithLargeBody);
    expect(bodyMaxLineLength8.status).toBe(1);
});

test("body-max-line-length9", () => {
    // see https://github.com/nblockchain/conventions/issues/124
    let commitMsgWithLargeBody = `GrpcService: fix some logging nits

These mistakes were made in 45faeca2f0e7c9c5545f54fb3fcc815f52b8a7cf.`;

    let bodyMaxLineLength9 = runCommitLintOnMsg(commitMsgWithLargeBody);
    expect(bodyMaxLineLength9.status).toBe(0);
});

test("body-max-line-length10", () => {
    // see https://github.com/nblockchain/conventions/issues/124
    let commitMsgWithLargeBody = `GrpcService: fix some logging nits

These mistakes were made in this GrpcService's RunIntoMeService commit: 45faeca2f0e7c9c5545f54fb3fcc815f52b8a7cf.`;

    let bodyMaxLineLength10 = runCommitLintOnMsg(commitMsgWithLargeBody);
    expect(bodyMaxLineLength10.status).toBe(1);
});

test("body-paragraph-line-min-length1", () => {
    let tenChars = "1234 67890";
    let fortyChars = tenChars + tenChars + tenChars + tenChars;
    let sixtyChars = fortyChars + tenChars + tenChars;
    let commitMsgWithFortyCharsInBody =
        "foo: this is only a title\n\n" + fortyChars + ".\n" + sixtyChars + ".";
    let bodyParagraphLineMinLength1 = runCommitLintOnMsg(
        commitMsgWithFortyCharsInBody
    );
    expect(bodyParagraphLineMinLength1.status).not.toBe(0);
});

test("body-paragraph-line-min-length2", () => {
    let commitMsgWithCommitUrlAtTheEndOfBodyParagraph = `foo: this is only a title

Foo bar:
https://github.com/username/repo/commit/1234567891234567891234567891234567891234`;

    let bodyParagraphLineMinLength2 = runCommitLintOnMsg(
        commitMsgWithCommitUrlAtTheEndOfBodyParagraph
    );

    expect(bodyParagraphLineMinLength2.status).toBe(0);
});

test("body-paragraph-line-min-length3", () => {
    let tenChars = "1234 67890";
    let fortyChars = tenChars + tenChars + tenChars + tenChars;
    let commitMsgWithFortyCharsInTheLastLineOfParagraph =
        "foo: this is only a title\n\n" + fortyChars + ".";
    let bodyParagraphLineMinLength3 = runCommitLintOnMsg(
        commitMsgWithFortyCharsInTheLastLineOfParagraph
    );
    expect(bodyParagraphLineMinLength3.status).toBe(0);
});

test("body-paragraph-line-min-length4", () => {
    let tenChars = "1234 67890";
    let fortyChars = tenChars + tenChars + tenChars + tenChars;
    let sixtyChars = fortyChars + tenChars + tenChars;
    let commitMsgWithCodeBlockThatSubceedsBodyMinLineLength =
        "foo: this is only a title" +
        "\n\n" +
        "Bar baz.\n```\n" +
        fortyChars +
        ".\n" +
        sixtyChars +
        "." +
        "\n```";

    let bodyParagraphLineMinLength4 = runCommitLintOnMsg(
        commitMsgWithCodeBlockThatSubceedsBodyMinLineLength
    );

    // because ```blocks surrounded like this``` can bypass the limit
    expect(bodyParagraphLineMinLength4.status).toBe(0);
});

test("body-paragraph-line-min-length5", () => {
    let commitMsgWithCoAuthoredByTagThatSubceedsBodyMinLineLength = `foo: this is only a title

Co-authored-by: Jon Doe <shortmail@example.com>
Co-authored-by: Jon Doe <JonDoeEmailAddress@example.com>`;

    let bodyParagraphLineMinLength5 = runCommitLintOnMsg(
        commitMsgWithCoAuthoredByTagThatSubceedsBodyMinLineLength
    );

    // because Co-authored-by tags can bypass the limit
    expect(bodyParagraphLineMinLength5.status).toBe(0);
});

test("body-paragraph-line-min-length6", () => {
    let commitMsgThatSubceedsBodyMinLineLength = `Some title of less than 50 chars

This is a paragraph whose 2nd line is less than 50 chars
but should not make commitlint complain because
TheNextWordInThe3rdLineIsTooLongToBePlacedIn2ndLine.`;

    let bodyParagraphLineMinLength6 = runCommitLintOnMsg(
        commitMsgThatSubceedsBodyMinLineLength
    );

    expect(bodyParagraphLineMinLength6.status).toBe(0);
});

test("body-paragraph-line-min-length7", () => {
    let commitMsgThatSubceedsBodyMinLineLengthButIsLegit = `Fixed bug (a title of less than 50 chars)

These were the steps to reproduce:
Do foo.

Current results:
Bar happens.

Expected results:
Baz happens.`;

    let bodyParagraphLineMinLength7 = runCommitLintOnMsg(
        commitMsgThatSubceedsBodyMinLineLengthButIsLegit
    );

    expect(bodyParagraphLineMinLength7.status).toBe(0);
});

test("body-paragraph-line-min-length8", () => {
    let commitMsgWithCodeBlockThatSubceedsBodyMinLineLength =
        "foo: this is only a title\n\n" +
        "Body with a link [1]:\n" +
        "```\n" +
        "some code block\n" +
        "```\n\n" +
        "[1] https://foo.com/bar";

    let bodyParagraphLineMinLength8 = runCommitLintOnMsg(
        commitMsgWithCodeBlockThatSubceedsBodyMinLineLength
    );

    expect(bodyParagraphLineMinLength8.status).toBe(0);
});

test("commit-hash-alone1", () => {
    let commitMsgWithCommitUrl = `foo: this is only a title

https://github.com/${process.env["GITHUB_REPOSITORY"]}/commit/3ee07243edc30604088a4b04ca525204ea440710`;

    let commitHashAlone1 = runCommitLintOnMsg(commitMsgWithCommitUrl);
    expect(commitHashAlone1.status).not.toBe(0);
});

test("commit-hash-alone2", () => {
    let commitMsgWithCommitHash = `foo: this is only a title

This is referring to [1] commit hash.

[1] 3ee07243edc30604088a4b04ca525204ea440710`;

    let commitHashAlone2 = runCommitLintOnMsg(commitMsgWithCommitHash);
    expect(commitHashAlone2.status).toBe(0);
});

test("commit-hash-alone3", () => {
    let commitMsgWithExternalCommitUrl = `foo: this is only a title

https://github.com/anotherOrg/anotherRepo/commit/3ee07243edc30604088a4b04ca525204ea440710`;

    let commitHashAlone3 = runCommitLintOnMsg(commitMsgWithExternalCommitUrl);
    expect(commitHashAlone3.status).toBe(0);
});

test("empty-wip-1", () => {
    let commitMsgWithEpmtyWIP = "WIP";
    let emptyWIP1 = runCommitLintOnMsg(commitMsgWithEpmtyWIP);
    expect(emptyWIP1.status).not.toBe(0);
});

test("empty-wip-2", () => {
    let commitMsgWithDescriptionAfterWIP = "WIP: bla bla blah";
    let emptyWIP2 = runCommitLintOnMsg(commitMsgWithDescriptionAfterWIP);
    expect(emptyWIP2.status).toBe(0);
});

test("empty-wip-3", () => {
    let commitMsgWithNumberAfterWIP = "WIP1";
    let emptyWIP3 = runCommitLintOnMsg(commitMsgWithNumberAfterWIP);
    expect(emptyWIP3.status).toBe(0);
});

test("footer-notes-misplacement-1", () => {
    let commitMsgWithRightFooter = `foo: this is only a title

Bla bla blah[1].

Fixes https://some/issue

[1] http://foo.bar/baz`;

    let footerNotesMisplacement1 = runCommitLintOnMsg(commitMsgWithRightFooter);
    expect(footerNotesMisplacement1.status).toBe(0);
});

test("footer-notes-misplacement-2", () => {
    let commitMsgWithWrongFooter = `foo: this is only a title

Fixes https://some/issue

Bla bla blah[1].

[1] http://foo.bar/baz`;

    let footerNotesMisplacement2 = runCommitLintOnMsg(commitMsgWithWrongFooter);
    expect(footerNotesMisplacement2.status).not.toBe(0);
});

test("footer-notes-misplacement-3", () => {
    let commitMsgWithWrongFooter = `foo: this is only a title

Bla bla blah[1]

[1] http://foo.bar/baz

Some other bla bla blah.

Fixes https://some/issue`;

    let footerNotesMisplacement3 = runCommitLintOnMsg(commitMsgWithWrongFooter);
    expect(footerNotesMisplacement3.status).not.toBe(0);
});

test("footer-notes-misplacement-4", () => {
    let commitMsgWithWrongFooter =
        "foo: this is only a title\n\n" +
        "Bla bla blah[1]:\n\n" +
        "```\nUnhandled Exception:\n--- Something between dashes ---\n```\n\n" +
        "[1] http://foo.bar/baz\n\n" +
        "Some other bla bla blah.\n\n" +
        "Fixes https://some/issue";

    let footerNotesMisplacement4 = runCommitLintOnMsg(commitMsgWithWrongFooter);
    expect(footerNotesMisplacement4.status).not.toBe(0);
});

test("footer-notes-misplacement-5", () => {
    let commitMsgWithRightFooter =
        "foo: this is only a title\n\n" +
        "Bla bla blah[1]:\n\n" +
        "```\nSome error message with a [] in the first of its line\n[warn] some warning\n```\n\n" +
        "[1] http://foo.bar/baz";
    let footerNotesMisplacement5 = runCommitLintOnMsg(commitMsgWithRightFooter);
    console.log(footerNotesMisplacement5.stdout.toString());
    expect(footerNotesMisplacement5.status).toBe(0);
});

test("footer-refs-validity1", () => {
    let commmitMsgWithCorrectFooter = `foo: this is only a title

Bla bla blah[1].

[1] http://foo.bar/baz`;

    let footerRefsValidity1 = runCommitLintOnMsg(commmitMsgWithCorrectFooter);
    expect(footerRefsValidity1.status).toBe(0);
});

test("footer-refs-validity2", () => {
    let commmitMsgWithWrongFooter = `foo: this is only a title

Bla bla blah.

[1] http://foo.bar/baz`;

    let footerRefsValidity2 = runCommitLintOnMsg(commmitMsgWithWrongFooter);
    expect(footerRefsValidity2.status).not.toBe(0);
});

test("footer-refs-validity3", () => {
    let commmitMsgWithWrongFooter = `foo: this is only a title

Bla bla blah[1], and after that [2], then [3].

[1] http://foo.bar/baz
[2] http://foo.bar/baz`;

    let footerRefsValidity3 = runCommitLintOnMsg(commmitMsgWithWrongFooter);
    expect(footerRefsValidity3.status).not.toBe(0);
});

test("footer-refs-validity4", () => {
    let commmitMsgWithFooter =
        "Backend/Ether: catch/retry new -32002 err code\n\n" +
        "CI on master branch caught this[1]:\n" +
        "```\nUnhandled Exception:\n" +
        "--- Something between dashes ---\n```\n" +
        "The end of the paragraph.\n\n" +
        "[1] https://github.com/nblockchain/geewallet/actions/runs/3507005645/jobs/5874411684";

    let footerRefsValidity4 = runCommitLintOnMsg(commmitMsgWithFooter);
    expect(footerRefsValidity4.status).toBe(0);
});

test("footer-refs-validity5", () => {
    let commmitMsgWithEOLFooter = `foo: this is only a title

Bla bla blah[1].

[1]
http://foo.bar/baz`;

    let footerRefsValidity5 = runCommitLintOnMsg(commmitMsgWithEOLFooter);
    expect(footerRefsValidity5.output[1].toString().includes("EOL")).toBe(true);
});

test("footer-refs-validity6", () => {
    let commitMsgWithUrlContainingAnchor = `foo: blah blah

Blah blah blah[1].

[1] https://somehost/somePath/someRes#7-some-numbered-anchor`;

    let footerRefsValidity6 = runCommitLintOnMsg(
        commitMsgWithUrlContainingAnchor
    );
    expect(footerRefsValidity6.status).toBe(0);
});

// This test reflects this issue: https://github.com/nblockchain/conventions/issues/125
test("footer-refs-validity7", () => {
    let commitMsgWithWithoutFooter = "foo: blah blah" + "\n\n" + "```[1]```";

    let footerRefsValidity7 = runCommitLintOnMsg(commitMsgWithWithoutFooter);
    expect(footerRefsValidity7.status).toBe(0);
});

// This test reflects this issue: https://github.com/nblockchain/conventions/issues/146
test("footer-refs-validity8", () => {
    let commitMsgWithCodeBlockAtFooterRef =
        "foo: blah blah" +
        "\n\n" +
        "Blah blah blah[1]." +
        "\n\n" +
        "[1]:\n" +
        "```\n" +
        "someCodeBlock\n" +
        "```";
    let footerRefsValidity8 = runCommitLintOnMsg(
        commitMsgWithCodeBlockAtFooterRef
    );
    expect(footerRefsValidity8.status).toBe(0);
});

test("prefer-slash-over-backslash1", () => {
    let commitMsgWithBackslash = "foo\\bar: bla bla bla";
    let preferSlashOverBackslash1 = runCommitLintOnMsg(commitMsgWithBackslash);
    expect(preferSlashOverBackslash1.status).not.toBe(0);
});

test("prefer-slash-over-backslash2", () => {
    let commitMsgWithSlash = "foo/bar: bla bla bla";
    let preferSlashOverBackslash2 = runCommitLintOnMsg(commitMsgWithSlash);
    expect(preferSlashOverBackslash2.status).toBe(0);
});

test("header-max-length-with-suggestions1", () => {
    let commitMsgWithThatExceedsHeaderMaxLength =
        "foo: this is only a title with a configuration in it that exceeds header max length";
    let headerMaxLength1 = runCommitLintOnMsg(
        commitMsgWithThatExceedsHeaderMaxLength
    );
    let expected_message = `"configuration" -> "config"`;
    expect(headerMaxLength1.status).not.toBe(0);
    expect((headerMaxLength1.stdout + "").includes(expected_message)).toEqual(
        true
    );
});

test("header-max-length-with-suggestions2", () => {
    let commitMsgWithThatExceedsHeaderMaxLength =
        "foo: this is only a title with a 1 second in it that exceeds header max length";
    let headerMaxLength2 = runCommitLintOnMsg(
        commitMsgWithThatExceedsHeaderMaxLength
    );
    let expected_message = `"1 second" -> "1sec"`;
    expect(headerMaxLength2.status).not.toBe(0);
    expect((headerMaxLength2.stdout + "").includes(expected_message)).toEqual(
        true
    );
});

test("header-max-length-with-suggestions3", () => {
    let commitMsgWithOnlyTwentySixCharsInTitle = "foo: this is only a title";
    let headerMaxLength3 = runCommitLintOnMsg(
        commitMsgWithOnlyTwentySixCharsInTitle
    );
    expect(headerMaxLength3.status).toBe(0);
});

test("header-max-length-with-suggestions4", () => {
    let tenChars = "1234 12345";
    let commitMsgWithOnlyFiftyCharsInTitle =
        "foo: 12345" + tenChars + tenChars + tenChars + tenChars;
    let headerMaxLength4 = runCommitLintOnMsg(
        commitMsgWithOnlyFiftyCharsInTitle
    );
    expect(headerMaxLength4.status).toBe(0);
});

test("header-max-length-with-suggestions5", () => {
    let longMergeCommitMessage =
        "Merge PR #42 from realmarv/fixFooterReferenceExistenceTruncatedBody";
    let headerMaxLength5 = runCommitLintOnMsg(longMergeCommitMessage);
    expect(headerMaxLength5.status).toBe(0);
});

test("header-max-length-with-suggestions6", () => {
    let commitMsgWithThatExceedsHeaderMaxLength =
        "Upgrade foo bla bla bla bla bla bla bla bla bla bla bla bla bla bla";
    let headerMaxLength6 = runCommitLintOnMsg(
        commitMsgWithThatExceedsHeaderMaxLength
    );
    let expected_message = `"upgrade" -> "update"`;
    expect(headerMaxLength6.status).not.toBe(0);
    expect((headerMaxLength6.stdout + "").includes(expected_message)).toEqual(
        true
    );
});

test("header-max-length-with-suggestions7", () => {
    let commitMsgWithThatExceedsHeaderMaxLength =
        "configure: this is the very very very very very long title";
    let headerMaxLength7 = runCommitLintOnMsg(
        commitMsgWithThatExceedsHeaderMaxLength
    );
    let expected_message = `"configure" -> "config"`;
    expect(headerMaxLength7.status).not.toBe(0);
    expect((headerMaxLength7.stdout + "").includes(expected_message)).toEqual(
        false
    );
});

test("header-max-length-with-suggestions8", () => {
    let commitMsgThatExceedsHeaderMaxLength =
        "Fix android build because blah blah very very very very very long title";
    let headerMaxLength8 = runCommitLintOnMsg(
        commitMsgThatExceedsHeaderMaxLength
    );
    let not_expected_message = `"and" -> "&"`;
    expect(headerMaxLength8.status).not.toBe(0);
    expect(
        (headerMaxLength8.stdout + "").includes(not_expected_message)
    ).toEqual(false);
});

test("header-max-length-with-suggestions9", () => {
    let commitMsgThatExceedsHeaderMaxLength =
        "title: 1 second bla bla bla bla bla bla bla bla bla bla bla bla bla bla";
    let headerMaxLength9 = runCommitLintOnMsg(
        commitMsgThatExceedsHeaderMaxLength
    );
    let expected_message = `"1 second" -> "1sec"`;
    expect(headerMaxLength9.status).not.toBe(0);
    expect((headerMaxLength9.stdout + "").includes(expected_message)).toEqual(
        true
    );
});

test("header-max-length-with-suggestions10", () => {
    let commitMsgThatExceedsHeaderMaxLength =
        "Configuration simplification bla bla bla bla bla bla bla bla bla bla bla";
    let headerMaxLength10 = runCommitLintOnMsg(
        commitMsgThatExceedsHeaderMaxLength
    );
    let expected_message = `"configuration" -> "config"`;
    expect(headerMaxLength10.status).not.toBe(0);
    expect((headerMaxLength10.stdout + "").includes(expected_message)).toEqual(
        true
    );
});

test("header-max-length-with-suggestions11", () => {
    let commitMsgThatExceedsHeaderMaxLength =
        "scope: 20 characters more because blah blah very very very very long title";
    let headerMaxLength11 = runCommitLintOnMsg(
        commitMsgThatExceedsHeaderMaxLength
    );
    let expected_message = `"characters" -> "chars"`;
    expect(headerMaxLength11.status).not.toBe(0);
    expect((headerMaxLength11.stdout + "").includes(expected_message)).toEqual(
        true
    );
});

test("header-max-length-with-suggestions12", () => {
    let commitMsgThatExceedsHeaderMaxLength =
        "Split that compares better because blah blah bla very very very long title";
    let headerMaxLength12 = runCommitLintOnMsg(
        commitMsgThatExceedsHeaderMaxLength
    );
    let not_expected_message = `"compares" -> "cmps"`;
    expect(headerMaxLength12.status).not.toBe(0);
    expect(
        (headerMaxLength12.stdout + "").includes(not_expected_message)
    ).toEqual(false);
});

test("proper-issue-refs1", () => {
    let commitMsgWithHashtagRef = `foo: blah blah

Blah blah #123.`;

    let properIssueRefs1 = runCommitLintOnMsg(commitMsgWithHashtagRef);
    expect(properIssueRefs1.status).not.toBe(0);
});

test("proper-issue-refs2", () => {
    let commitMsgWithFullUrl = `foo: blah blah

Fixes someUrl://blah.blah/158`;

    let properIssueRefs2 = runCommitLintOnMsg(commitMsgWithFullUrl);
    expect(properIssueRefs2.status).toBe(0);
});

test("proper-issue-refs3", () => {
    let commitMsgWithHashtagRefInBlock =
        "foo: this is only a title" +
        "\n\n" +
        "Bar baz:\n\n```\ntype Foo = string #123\n```";
    let properIssueRefs3 = runCommitLintOnMsg(commitMsgWithHashtagRefInBlock);
    expect(properIssueRefs3.status).toBe(0);
});

test("proper-issue-refs4", () => {
    let commitMsgWithFullUrl = `foo: blah blah

Some paragraph text with a ref[1].

[1] someUrl://someHostName/someFolder/someResource#666-anchor`;

    let properIssueRefs4 = runCommitLintOnMsg(commitMsgWithFullUrl);
    expect(properIssueRefs4.status).toBe(0);
});

test("proper-issue-refs5", () => {
    let commitMsgWithHashtagRef = `foo: blah blah

#123 bug is fixed.`;

    let properIssueRefs5 = runCommitLintOnMsg(commitMsgWithHashtagRef);
    expect(properIssueRefs5.status).not.toBe(0);
});

test("subject-lowercase1", () => {
    let commitMsgWithUppercaseAfterColon = "foo: Bar baz";
    let subjectLowerCase1 = runCommitLintOnMsg(
        commitMsgWithUppercaseAfterColon
    );
    expect(subjectLowerCase1.status).not.toBe(0);
});

test("subject-lowercase2", () => {
    let commitMsgWithLowercaseAfterColon = "foo: bar baz";
    let subjectLowerCase2 = runCommitLintOnMsg(
        commitMsgWithLowercaseAfterColon
    );
    expect(subjectLowerCase2.status).toBe(0);
});

test("subject-lowercase3", () => {
    let commitMsgWithAcronymAfterColon = "foo: BAR baz";
    let subjectLowerCase3 = runCommitLintOnMsg(commitMsgWithAcronymAfterColon);
    expect(subjectLowerCase3.status).toBe(0);
});

test("subject-lowercase4", () => {
    let commitMsgWithNonAlphanumericAfterColon = "foo: 3 tests added";
    let subjectLowerCase4 = runCommitLintOnMsg(
        commitMsgWithNonAlphanumericAfterColon
    );
    expect(subjectLowerCase4.status).toBe(0);
});

test("subject-lowercase5", () => {
    let commitMsgWithRareCharInScope1 = "foo.bar: Baz";
    let subjectLowerCase5 = runCommitLintOnMsg(commitMsgWithRareCharInScope1);
    expect(subjectLowerCase5.status).not.toBe(0);
});

test("subject-lowercase6", () => {
    let commitMsgWithRareCharInScope2 = "foo-bar: Baz";
    let subjectLowerCase6 = runCommitLintOnMsg(commitMsgWithRareCharInScope2);
    expect(subjectLowerCase6.status).not.toBe(0);
});

test("subject-lowercase7", () => {
    let commitMsgWithRareCharInScope3 = "foo,bar: Baz";
    let subjectLowerCase7 = runCommitLintOnMsg(commitMsgWithRareCharInScope3);
    expect(subjectLowerCase7.status).not.toBe(0);
});

test("subject-lowercase8", () => {
    let commitMsgWithPascalCaseAfterColon =
        "End2End: TestFixtureSetup refactor";
    let subjectLowerCase8 = runCommitLintOnMsg(
        commitMsgWithPascalCaseAfterColon
    );
    expect(subjectLowerCase8.status).toBe(0);
});

test("subject-lowercase9", () => {
    let commitMsgWithCamelCaseAfterColon = "End2End: testFixtureSetup refactor";
    let subjectLowerCase9 = runCommitLintOnMsg(
        commitMsgWithCamelCaseAfterColon
    );
    expect(subjectLowerCase9.status).toBe(0);
});

test("subject-lowercase10", () => {
    let commitMsgWithNumber = "foo: A1 bar";
    let subjectLowerCase10 = runCommitLintOnMsg(commitMsgWithNumber);
    expect(subjectLowerCase10.status).toBe(0);
});

test("title-uppercase1", () => {
    let commitMsgWithoutScope = "remove logs";
    let titleUpperCase1 = runCommitLintOnMsg(commitMsgWithoutScope);
    expect(titleUpperCase1.status).not.toBe(0);
});

test("title-uppercase2", () => {
    let commitMsgWithoutScope = "Remove logs";
    let titleUpperCase2 = runCommitLintOnMsg(commitMsgWithoutScope);
    expect(titleUpperCase2.status).toBe(0);
});

test("title-uppercase3", () => {
    let commitMsgWithoutScope = "testFixtureSetup refactor";
    let titleUpperCase3 = runCommitLintOnMsg(commitMsgWithoutScope);
    expect(titleUpperCase3.status).toBe(0);
});

test("title-uppercase4", () => {
    let commitMsgWithLowerCaseScope = "lowercase: scope is lowercase";
    let titleUpperCase4 = runCommitLintOnMsg(commitMsgWithLowerCaseScope);
    expect(titleUpperCase4.status).toBe(0);
});

test("too-many-spaces1", () => {
    let commitMsgWithTooManySpacesInTitle = "foo: this is only a  title";
    let tooManySpaces1 = runCommitLintOnMsg(commitMsgWithTooManySpacesInTitle);
    expect(tooManySpaces1.status).not.toBe(0);
});

test("too-many-spaces2", () => {
    let commitMsgWithTooManySpacesInBody = `foo: this is only a title

Bla  blah bla.`;

    let tooManySpaces2 = runCommitLintOnMsg(commitMsgWithTooManySpacesInBody);
    expect(tooManySpaces2.status).not.toBe(0);
});

test("too-many-spaces3", () => {
    let commitMsgWithTooManySpacesInCodeBlock =
        "foo: this is only a title\n\n" +
        "Bar baz:\n\n```\ntype   Foo =\nstring\n```";
    let tooManySpaces3 = runCommitLintOnMsg(
        commitMsgWithTooManySpacesInCodeBlock
    );
    expect(tooManySpaces3.status).toBe(0);
});

test("too-many-spaces4", () => {
    let commitMsgWithTwoSpacesAfterSentence = `foo: this is only a title

Bla blah.  Blah bla.`;

    let tooManySpaces4 = runCommitLintOnMsg(
        commitMsgWithTwoSpacesAfterSentence
    );
    expect(tooManySpaces4.status).toBe(0);
});

test("too-many-spaces5", () => {
    let commitMsgWithThreeSpacesAfterSentence = `foo: this is only a title

Bla blah.   Blah bla.`;

    let tooManySpaces5 = runCommitLintOnMsg(
        commitMsgWithThreeSpacesAfterSentence
    );
    expect(tooManySpaces5.status).not.toBe(0);
});

test("trailing-whitespace1", () => {
    let commitMsgWithNoTrailingWhiteSpace = `foo: this is only a title

Bla blah bla.`;

    let trailingWhitespace1 = runCommitLintOnMsg(
        commitMsgWithNoTrailingWhiteSpace
    );
    expect(trailingWhitespace1.status).toBe(0);
});

test("trailing-whitespace2", () => {
    let commitMsgWithTrailingWhiteSpaceInTitleEnd = `foo: title 

Bla blah bla.`;

    let trailingWhitespace2 = runCommitLintOnMsg(
        commitMsgWithTrailingWhiteSpaceInTitleEnd
    );
    expect(trailingWhitespace2.status).not.toBe(0);
});

test("trailing-whitespace3", () => {
    let commitMsgWithTrailingWhiteSpaceInTitleStart = ` foo: title

Bla blah bla.`;

    let trailingWhitespace3 = runCommitLintOnMsg(
        commitMsgWithTrailingWhiteSpaceInTitleStart
    );
    expect(trailingWhitespace3.status).not.toBe(0);
});

test("trailing-whitespace4", () => {
    let commitMsgWithTrailingWhiteSpaceInBodyStart = `foo: title

 Bla blah bla.`;

    let trailingWhitespace4 = runCommitLintOnMsg(
        commitMsgWithTrailingWhiteSpaceInBodyStart
    );
    expect(trailingWhitespace4.status).not.toBe(0);
});

test("trailing-whitespace5", () => {
    let commitMsgWithTrailingWhiteSpaceInBodyEnd = `foo: title

Bla blah bla. `;

    let trailingWhitespace5 = runCommitLintOnMsg(
        commitMsgWithTrailingWhiteSpaceInBodyEnd
    );
    expect(trailingWhitespace5.status).not.toBe(0);
});

test("trailing-whitespace6", () => {
    let commitMsgWithTrailingWhiteSpaceInCodeBlock =
        "foo: this is only a title" +
        "\n\n" +
        "Bar baz:\n\n```\ntype Foo =\n    string\n```";
    let trailingWhitespace6 = runCommitLintOnMsg(
        commitMsgWithTrailingWhiteSpaceInCodeBlock
    );
    expect(trailingWhitespace6.status).toBe(0);
});

test("type-space-after-colon1", () => {
    let commitMsgWithNoSpace = "foo:bar";
    let typeSpaceAfterColon1 = runCommitLintOnMsg(commitMsgWithNoSpace);
    expect(typeSpaceAfterColon1.status).not.toBe(0);
});

test("type-space-after-colon2", () => {
    let commitMsgWithSpace = "foo: bar";
    let typeSpaceAfterColon2 = runCommitLintOnMsg(commitMsgWithSpace);
    expect(typeSpaceAfterColon2.status).toBe(0);
});

test("type-space-after-colon3", () => {
    let commitMsgWithNoSpaceBeforeColonButAtTheEnd = "foo: a tale of bar:baz";
    let typeSpaceAfterColon3 = runCommitLintOnMsg(
        commitMsgWithNoSpaceBeforeColonButAtTheEnd
    );
    expect(typeSpaceAfterColon3.status).toBe(0);
});

test("type-space-after-comma1", () => {
    let commitMsgWithSpaceAfterCommaInType = "foo, bar: bla bla blah";
    let typeSpaceAfterComma1 = runCommitLintOnMsg(
        commitMsgWithSpaceAfterCommaInType
    );
    expect(typeSpaceAfterComma1.status).not.toBe(0);
});

test("type-space-after-comma2", () => {
    let commitMsgWithNoSpaceAfterCommaInType = "foo,bar: bla bla blah";
    let typeSpaceAfterComma2 = runCommitLintOnMsg(
        commitMsgWithNoSpaceAfterCommaInType
    );
    expect(typeSpaceAfterComma2.status).toBe(0);
});

test("type-space-before-paren1", () => {
    let commitMsgWithNoSpaceBeforeParen = "foo (bar): bla bla bla";
    let typeSpaceBeforeParen1 = runCommitLintOnMsg(
        commitMsgWithNoSpaceBeforeParen
    );
    expect(typeSpaceBeforeParen1.status).not.toBe(0);
});

test("type-space-before-paren2", () => {
    let commitMsgWithNoSpaceBeforeParen = "foo(bar): bla bla bla";
    let typeSpaceBeforeParen2 = runCommitLintOnMsg(
        commitMsgWithNoSpaceBeforeParen
    );
    expect(typeSpaceBeforeParen2.status).toBe(0);
});

test("type-space-before-paren3", () => {
    let commitMsgWithNoSpaceBeforeParen = "(bar): bla bla bla";
    let typeSpaceBeforeParen3 = runCommitLintOnMsg(
        commitMsgWithNoSpaceBeforeParen
    );
    expect(typeSpaceBeforeParen3.status).toBe(0);
});

test("type-with-square-brackets1", () => {
    let commitMsgWithSquareBrackets = "[foo] this is a title";
    let typeWithSquareBrackets1 = runCommitLintOnMsg(
        commitMsgWithSquareBrackets
    );
    expect(typeWithSquareBrackets1.status).not.toBe(0);
});

test("type-with-square-brackets2", () => {
    let commitMsgWithoutSquareBrackets = "foo: this is a title";
    let typeWithSquareBrackets2 = runCommitLintOnMsg(
        commitMsgWithoutSquareBrackets
    );
    expect(typeWithSquareBrackets2.status).toBe(0);
});
