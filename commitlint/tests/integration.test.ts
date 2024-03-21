import { runCommitLintOnMsg } from "./testHelpers.js";
import { test, expect } from "vitest";

test("body-leading-blank1", () => {
    let commitMsgWithoutEmptySecondLine =
        "foo: this is only a title" + "\n" + "Bar baz.";
    let bodyLeadingBlank1 = runCommitLintOnMsg(commitMsgWithoutEmptySecondLine);
    expect(bodyLeadingBlank1.status).not.toBe(0);
});

test("subject-full-stop1", () => {
    let commitMsgWithEndingDotInTitle = "foo/bar: bla bla blah.";
    let subjectFullStop1 = runCommitLintOnMsg(commitMsgWithEndingDotInTitle);
    expect(subjectFullStop1.status).not.toBe(0);
});

test("subject-full-stop2", () => {
    let commitMsgWithoutEndingDotInTitle = "foo/bar: bla bla blah";
    let subjectFullStop2 = runCommitLintOnMsg(commitMsgWithoutEndingDotInTitle);
    expect(subjectFullStop2.status).toBe(0);
});
