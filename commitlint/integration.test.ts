let cp = require("child_process");

function runCommitLintOnMsg(inputMsg: string) {
    return cp.spawnSync("npx", ["commitlint", "--verbose"], {
        input: inputMsg,
    });
}

test("body-leading-blank1", () => {
    let commitMsgWithoutEmptySecondLine =
        "foo: this is only a title" + "\n" + "Bar baz.";
    let bodyLeadingBlank1 = runCommitLintOnMsg(commitMsgWithoutEmptySecondLine);
    expect(bodyLeadingBlank1.status).not.toBe(0);
});
