import { test, expect } from "vitest";
import { None, Some, Option, OptionStatic } from "../fpHelpers.js";
const { spawnSync } = require("child_process");
const os = require("os");

function typeGuard(option: Option<number>) {
    if (option instanceof None) {
        return "NAH";
    } else {
        let val = option.value;
        return (val * val).toString();
    }
}

function ofObj1(option: number | null): Option<number> {
    let foo = OptionStatic.OfObj(option);
    return foo;
}

function ofObj2(option: number | undefined): Option<number> {
    let foo = OptionStatic.OfObj(option);
    return foo;
}

test("testing Options", () => {
    let foo: Option<number> = new None();
    let bar: Option<number> = new Some(2);
    expect(typeGuard(foo)).toBe("NAH");
    expect(typeGuard(bar)).toBe("4");
});

test("testing Is methods", () => {
    let foo: Option<number> = OptionStatic.None;
    let bar: Option<number> = new Some(2);
    expect(foo.IsNone()).toBe(true);
    expect(bar.IsNone()).toBe(false);
    expect(foo.IsSome()).toBe(false);
    expect(bar.IsSome()).toBe(true);
});

test("testing OfObj", () => {
    let two: number | null = 2;
    expect(typeGuard(ofObj1(two))).toBe("4");
    two = null;
    expect(typeGuard(ofObj1(two))).toBe("NAH");

    let four: number | undefined = 4;
    expect(typeGuard(ofObj2(four))).toBe("16");
    four = undefined;
    expect(typeGuard(ofObj2(four))).toBe("NAH");
});

export function runCommitLintOnMsg(inputMsg: string) {
    // FIXME: should we .lowerCase().startsWith("win") in case it starts
    // returning Win64 in the future? thing is, our CI doesn't like this
    // change (strangely enough it causes commitlint test suite to fail)
    const command = os.platform() === "win32" ? "npx.cmd" : "npx";
    return spawnSync(command, ["commitlint", "--verbose"], { input: inputMsg });
}
