import { runCommitLintOnMsg } from "./testHelpers.js";
import { test, expect } from "vitest";

class A {
    x: string;
    y: string;

    constructor() {
        this.x = "hello";
        this.y = "hallo";
    }
}
class B {
    x: number;
    y: number;

    constructor() {
        this.x = 1;
        this.y = 2;
    }
}

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

type C = A | B;
function fn(p: C) {
    if (p instanceof A) {
        return p.x + p.y;
    }
    if (p instanceof B) {
        let sub = p.y - p.x;
        return sub.toString();
    }
}

test("testing DUs", () => {
    let foo = new A();
    expect(foo.x).toBe("hello");
    expect(foo.y).toBe("hallo");
    let bar = new B();
    expect(bar.x).toBe(1);
    expect(bar.y).toBe(2);
});
