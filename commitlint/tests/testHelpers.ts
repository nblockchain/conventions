import { test, expect } from "vitest";
import { None, Some, Option } from "../fpHelpers.js";
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

test("testing Options", () => {
    let foo: Option<number> = new None();
    let bar: Option<number> = new Some(2);
    expect(typeGuard(foo)).toBe("NAH");
    expect(typeGuard(bar)).toBe("4");
});

export function runCommitLintOnMsg(inputMsg: string) {
    // FIXME: should we .lowerCase().startsWith("win") in case it starts
    // returning Win64 in the future? thing is, our CI doesn't like this
    // change (strangely enough it causes commitlint test suite to fail)
    const command = os.platform() === "win32" ? "npx.cmd" : "npx";
    return spawnSync(command, ["commitlint", "--verbose"], { input: inputMsg });
}
