const { spawnSync } = require("child_process");
const os = require("os");

export function runCommitLintOnMsg(inputMsg: string) {
    const command = os.platform() === "win32" ? "npx.cmd" : "npx";
    return spawnSync(command, ["commitlint", "--verbose"], { input: inputMsg });
}
