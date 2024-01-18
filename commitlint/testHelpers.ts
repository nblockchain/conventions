const { spawnSync } = require("child_process");

export function runCommitLintOnMsg(inputMsg: string) {
    return spawnSync("npx", ["commitlint", "--verbose"], { input: inputMsg });
}
