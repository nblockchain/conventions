const { spawnSync } = require("child_process");
const os = require("os");

export function runCommitLintOnMsg(inputMsg: string) {
    const command =
        // FIXME: should we .lowerCase().startsWith("win") in case it starts
        // returning Win64 in the future? thing is, our CI doesn't like this
        // change (strangely enough it causes commitlint test suite to fail)
        os.platform() === "win32" ? ".\\commitlint.bat" : "./commitlint.sh";
    return spawnSync(command, ["--verbose"], { input: inputMsg });
}
