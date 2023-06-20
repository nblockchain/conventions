import { Helpers } from "./commitlint/helpers";
import { Plugins } from "./commitlint/plugins";
import { RuleConfigSeverity } from "@commitlint/types";

let bodyMaxLineLength = 64;
let headerMaxLineLength = 50;
let footerMaxLineLength = 150;

function notNullStringErrorMessage(stringType: string): string {
    return `This is unexpected because ${stringType} should never be null`;
}

module.exports = {
    parserPreset: "conventional-changelog-conventionalcommits",
    rules: {
        "body-leading-blank": [RuleConfigSeverity.Error, "always"],
        "body-soft-max-line-length": [
            RuleConfigSeverity.Error,
            "always",
            bodyMaxLineLength,
        ],
        "body-paragraph-line-min-length": [RuleConfigSeverity.Error, "always"],
        "empty-wip": [RuleConfigSeverity.Error, "always"],
        "footer-leading-blank": [RuleConfigSeverity.Warning, "always"],
        "footer-max-line-length": [
            RuleConfigSeverity.Error,
            "always",
            footerMaxLineLength,
        ],
        "footer-notes-misplacement": [RuleConfigSeverity.Error, "always"],
        "footer-refs-validity": [RuleConfigSeverity.Error, "always"],
        "header-max-length-with-suggestions": [
            RuleConfigSeverity.Error,
            "always",
            headerMaxLineLength,
        ],
        "subject-full-stop": [RuleConfigSeverity.Error, "never", "."],
        "type-space-after-colon": [RuleConfigSeverity.Error, "always"],
        "subject-lowercase": [RuleConfigSeverity.Error, "always"],
        "body-prose": [RuleConfigSeverity.Error, "always"],
        "type-space-after-comma": [RuleConfigSeverity.Error, "always"],
        "trailing-whitespace": [RuleConfigSeverity.Error, "always"],
        "prefer-slash-over-backslash": [RuleConfigSeverity.Error, "always"],
        "type-space-before-paren": [RuleConfigSeverity.Error, "always"],
        "type-with-square-brackets": [RuleConfigSeverity.Error, "always"],
        /* disabled until a related problem is fixed first in another rule
        "proper-issue-refs": [RuleConfigSeverity.Error, "always"],
        */
        "too-many-spaces": [RuleConfigSeverity.Error, "always"],
        "commit-hash-alone": [RuleConfigSeverity.Error, "always"],
        "title-uppercase": [RuleConfigSeverity.Error, "always"],

        // disabled because most of the time it doesn't work, due to https://github.com/conventional-changelog/commitlint/issues/3404
        // and anyway we were using this rule only as a warning, not an error (because a scope is not required, e.g. when too broad)
        "type-empty": [RuleConfigSeverity.Disabled, "never"],
    },
    plugins: [
        // TODO (ideas for more rules):
        // * Detect reverts which have not been elaborated.
        // * Reject some stupid obvious words: change, update, modify (if first word after colon, error; otherwise warning).
        // * Think of how to reject this shitty commit message: https://github.com/nblockchain/NOnion/pull/34/commits/9ffcb373a1147ed1c729e8aca4ffd30467255594
        // * Workflow: detect if wip commit in a branch not named "wip/*" or whose name contains "squashed".
        // * Detect if commit hash mention in commit msg actually exists in repo.
        // * Detect scope(sub-scope) in the title that doesn't include scope part (e.g., writing (bar) instead of foo(bar))

        {
            rules: {
                "body-prose": ({ raw }: { raw: any }) => {
                    let rawStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(raw, "raw"),
                        notNullStringErrorMessage("raw")
                    );

                    return Plugins.bodyProse(rawStr);
                },

                "commit-hash-alone": ({ raw }: { raw: any }) => {
                    let rawStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(raw, "raw"),
                        notNullStringErrorMessage("raw")
                    );

                    return Plugins.commitHashAlone(rawStr);
                },

                "empty-wip": ({ header }: { header: any }) => {
                    let headerStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(header, "header"),
                        notNullStringErrorMessage("header")
                    );

                    return Plugins.emptyWip(headerStr);
                },

                "header-max-length-with-suggestions": (
                    { header }: { header: any },
                    _: any,
                    maxLineLength: number
                ) => {
                    let headerStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(header, "header"),
                        notNullStringErrorMessage("header")
                    );

                    return Plugins.headerMaxLengthWithSuggestions(
                        headerStr,
                        maxLineLength
                    );
                },

                "footer-notes-misplacement": ({ body }: { body: any }) => {
                    let bodyStr = Helpers.convertAnyToString(body, "body");
                    return Plugins.footerNotesMisplacement(bodyStr);
                },

                "footer-refs-validity": ({ raw }: { raw: any }) => {
                    let rawStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(raw, "raw"),
                        notNullStringErrorMessage("raw")
                    );

                    return Plugins.footerRefsValidity(rawStr);
                },

                "prefer-slash-over-backslash": ({
                    header,
                }: {
                    header: any;
                }) => {
                    let headerStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(header, "header"),
                        notNullStringErrorMessage("header")
                    );

                    return Plugins.preferSlashOverBackslash(headerStr);
                },

                "proper-issue-refs": ({ raw }: { raw: any }) => {
                    let rawStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(raw, "raw"),
                        notNullStringErrorMessage("raw")
                    );

                    return Plugins.properIssueRefs(rawStr);
                },

                "title-uppercase": ({ header }: { header: any }) => {
                    let headerStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(header, "header"),
                        notNullStringErrorMessage("header")
                    );

                    return Plugins.titleUppercase(headerStr);
                },

                "too-many-spaces": ({ raw }: { raw: any }) => {
                    let rawStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(raw, "raw"),
                        notNullStringErrorMessage("raw")
                    );

                    return Plugins.tooManySpaces(rawStr);
                },

                "type-space-after-colon": ({ header }: { header: any }) => {
                    let headerStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(header, "header"),
                        notNullStringErrorMessage("header")
                    );

                    return Plugins.typeSpaceAfterColon(headerStr);
                },

                "type-with-square-brackets": ({ header }: { header: any }) => {
                    let headerStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(header, "header"),
                        notNullStringErrorMessage("header")
                    );

                    return Plugins.typeWithSquareBrackets(headerStr);
                },

                // NOTE: we use 'header' instead of 'subject' as a workaround to this bug: https://github.com/conventional-changelog/commitlint/issues/3404
                "subject-lowercase": ({ header }: { header: any }) => {
                    let headerStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(header, "header"),
                        notNullStringErrorMessage("header")
                    );
                    return Plugins.subjectLowercase(headerStr);
                },

                "type-space-after-comma": ({ header }: { header: any }) => {
                    let headerStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(header, "header"),
                        notNullStringErrorMessage("header")
                    );

                    return Plugins.typeSpaceAfterComma(headerStr);
                },

                "body-soft-max-line-length": (
                    { body }: { body: any },
                    _: any,
                    maxLineLength: number
                ) => {
                    let bodyStr = Helpers.convertAnyToString(body, "body");
                    return Plugins.bodySoftMaxLineLength(
                        bodyStr,
                        maxLineLength
                    );
                },

                "body-paragraph-line-min-length": ({ body }: { body: any }) => {
                    let bodyStr = Helpers.convertAnyToString(body, "body");
                    return Plugins.bodyParagraphLineMinLength(
                        bodyStr,
                        headerMaxLineLength,
                        bodyMaxLineLength
                    );
                },

                "trailing-whitespace": ({ raw }: { raw: any }) => {
                    let rawStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(raw, "raw"),
                        notNullStringErrorMessage("raw")
                    );

                    return Plugins.trailingWhitespace(rawStr);
                },

                "type-space-before-paren": ({ header }: { header: any }) => {
                    let headerStr = Helpers.assertNotNull(
                        Helpers.convertAnyToString(header, "header"),
                        notNullStringErrorMessage("header")
                    );

                    return Plugins.typeSpaceBeforeParen(headerStr);
                },
            },
        },
    ],
};
