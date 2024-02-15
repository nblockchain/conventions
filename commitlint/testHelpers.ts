const { spawnSync } = require("child_process");
const os = require("os");

export function runCommitLintOnMsg(inputMsg: string) {
    const command =
        os.platform() === "win32" ? ".\\commitlint.bat" : "./commitlint.sh";
    return spawnSync(command, ["--verbose"], { input: inputMsg });
}
