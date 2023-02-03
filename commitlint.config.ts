import { Helpers } from "./commitlint/helpers";
import { Plugins } from "./commitlint/plugins";

enum RuleStatus {
    Disabled = 0,
    Warning = 1,
    Error = 2,
}

let bodyMaxLineLength = 64;
let headerMaxLineLength = 50;

module.exports = {
    parserPreset: "conventional-changelog-conventionalcommits",
    rules: {
        "body-leading-blank": [RuleStatus.Warning, "always"],
        "body-soft-max-line-length": [
            RuleStatus.Error,
            "always",
            bodyMaxLineLength,
        ],
        "empty-wip": [RuleStatus.Error, "always"],
        "footer-leading-blank": [RuleStatus.Warning, "always"],
        "footer-max-line-length": [RuleStatus.Error, "always", 150],
        "footer-notes-misplacement": [RuleStatus.Error, "always"],
        "footer-references-existence": [RuleStatus.Error, "always"],
        "header-max-length-with-suggestions": [
            RuleStatus.Error,
            "always",
            headerMaxLineLength,
        ],
        "subject-full-stop": [RuleStatus.Error, "never", "."],
        "type-empty": [RuleStatus.Warning, "never"],
        "type-space-after-colon": [RuleStatus.Error, "always"],
        "subject-lowercase": [RuleStatus.Error, "always"],
        "body-prose": [RuleStatus.Error, "always"],
        "type-space-after-comma": [RuleStatus.Error, "always"],
        "trailing-whitespace": [RuleStatus.Error, "always"],
        "prefer-slash-over-backslash": [RuleStatus.Error, "always"],
        "type-space-before-paren": [RuleStatus.Error, "always"],
        "type-with-square-brackets": [RuleStatus.Error, "always"],
        "proper-issue-refs": [RuleStatus.Error, "always"],
        "too-many-spaces": [RuleStatus.Error, "always"],
        "commit-hash-alone": [RuleStatus.Error, "always"],
        "title-uppercase": [RuleStatus.Error, "always"],
    },
    plugins: [
        // TODO (ideas for more rules):
        // * Detect if paragraphs in body have been cropped too shortly (less than 64 chars), and suggest same auto-wrap command that body-soft-max-line-length suggests, since it unwraps and wraps (both).
        // * Detect reverts which have not been elaborated.
        // * Reject some stupid obvious words: change, update, modify (if first word after colon, error; otherwise warning).
        // * Think of how to reject this shitty commit message: https://github.com/nblockchain/NOnion/pull/34/commits/9ffcb373a1147ed1c729e8aca4ffd30467255594
        // * Title should not have dot at the end.
        // * Second line of commit msg should always be blank.
        // * Workflow: detect if wip commit in a branch not named "wip/*" or whose name contains "squashed".
        // * Detect if commit hash mention in commit msg actually exists in repo.
        // * Detect area(sub-area) in the title that doesn't include area part (e.g., writing (bar) instead of foo(bar))

        {
            rules: {
                "body-prose": ({ raw }: { raw: any }) => {
                    let rawStr = Helpers.convertAnyToString(raw, "raw").trim();
                    return Plugins.bodyProse(rawStr);
                },

                "commit-hash-alone": ({ raw }: { raw: any }) => {
                    let rawStr = Helpers.convertAnyToString(raw, "raw");
                    return Plugins.commitHashAlone(rawStr);
                },

                "empty-wip": ({ header }: { header: any }) => {
                    let headerStr = Helpers.convertAnyToString(
                        header,
                        "header"
                    );
                    return Plugins.emptyWip(headerStr);
                },

                "header-max-length-with-suggestions": (
                    { header }: { header: any },
                    _: any,
                    maxLineLength: number
                ) => {
                    let headerStr = Helpers.convertAnyToString(
                        header,
                        "header"
                    );
                    return Plugins.headerMaxLengthWithSuggestions(
                        headerStr,
                        maxLineLength
                    );
                },

                "footer-notes-misplacement": ({ raw }: { raw: any }) => {
                    let rawStr = Helpers.convertAnyToString(raw, "raw").trim();
                    return Plugins.footerNotesMisplacement(rawStr);
                },

                "footer-references-existence": ({ raw }: { raw: any }) => {
                    let rawStr = Helpers.convertAnyToString(raw, "raw").trim();
                    return Plugins.footerReferencesExistence(rawStr);
                },

                "prefer-slash-over-backslash": ({
                    header,
                }: {
                    header: any;
                }) => {
                    let headerStr = Helpers.convertAnyToString(
                        header,
                        "header"
                    );
                    return Plugins.preferSlashOverBackslash(headerStr);
                },

                "proper-issue-refs": ({ raw }: { raw: any }) => {
                    let rawStr = Helpers.convertAnyToString(raw, "raw").trim();
                    return Plugins.properIssueRefs(rawStr);
                },

                "title-uppercase": ({ header }: { header: any }) => {
                    let headerStr = Helpers.convertAnyToString(
                        header,
                        "header"
                    );
                    return Plugins.titleUppercase(headerStr);
                },

                "too-many-spaces": ({ raw }: { raw: any }) => {
                    let rawStr = Helpers.convertAnyToString(raw, "raw");
                    return Plugins.tooManySpaces(rawStr);
                },

                "type-space-after-colon": ({ header }: { header: any }) => {
                    let headerStr = Helpers.convertAnyToString(
                        header,
                        "header"
                    );
                    return Plugins.typeSpaceAfterColon(headerStr);
                },

                "type-with-square-brackets": ({ header }: { header: any }) => {
                    let headerStr = Helpers.convertAnyToString(
                        header,
                        "header"
                    );
                    return Plugins.typeWithSquareBrackets(headerStr);
                },

                // NOTE: we use 'header' instead of 'subject' as a workaround to this bug: https://github.com/conventional-changelog/commitlint/issues/3404
                "subject-lowercase": ({ header }: { header: any }) => {
                    let headerStr = Helpers.convertAnyToString(
                        header,
                        "header"
                    );
                    return Plugins.subjectLowercase(headerStr);
                },

                "type-space-after-comma": ({ header }: { header: any }) => {
                    let headerStr = Helpers.convertAnyToString(
                        header,
                        "header"
                    );
                    return Plugins.typeSpaceAfterComma(headerStr);
                },

                "body-soft-max-line-length": (
                    { raw }: { raw: any },
                    _: any,
                    maxLineLength: number
                ) => {
                    let rawStr = Helpers.convertAnyToString(raw, "raw").trim();
                    return Plugins.bodySoftMaxLineLength(rawStr, maxLineLength);
                },

                "trailing-whitespace": ({ raw }: { raw: any }) => {
                    let rawStr = Helpers.convertAnyToString(raw, "raw");
                    return Plugins.trailingWhitespace(rawStr);
                },

                "type-space-before-paren": ({ header }: { header: any }) => {
                    let headerStr = Helpers.convertAnyToString(
                        header,
                        "header"
                    );
                    return Plugins.typeSpaceBeforeParen(headerStr);
                },
            },
        },
    ],
};
