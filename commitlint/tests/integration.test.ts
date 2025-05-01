import { runCommitLintOnMsg } from "./testHelpers.js";
import { test, expect } from "vitest";

test("body-leading-blank1", () => {
    const commitMsgWithoutEmptySecondLine =
        "foo: this is only a title" + "\n" + "Bar baz.";
    const bodyLeadingBlank1 = runCommitLintOnMsg(
        commitMsgWithoutEmptySecondLine
    );
    expect(bodyLeadingBlank1.status).not.toBe(0);
});

test("subject-full-stop1", () => {
    const commitMsgWithEndingDotInTitle = "foo/bar: bla bla blah.";
    const subjectFullStop1 = runCommitLintOnMsg(commitMsgWithEndingDotInTitle);
    expect(subjectFullStop1.status).not.toBe(0);
});

test("subject-full-stop2", () => {
    const commitMsgWithoutEndingDotInTitle = "foo/bar: bla bla blah";
    const subjectFullStop2 = runCommitLintOnMsg(
        commitMsgWithoutEndingDotInTitle
    );
    expect(subjectFullStop2.status).toBe(0);
});
