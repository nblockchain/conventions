import { Helpers } from "./helpers";

export abstract class Plugins {
    public static commitHashAlone(rawStr: string) {
        let offence = false;

        let urls = Helpers.findUrls(rawStr)

        let gitRepo = process.env['GITHUB_REPOSITORY'];
        if (gitRepo !== undefined && urls !== null) {
            for (let url of urls.entries()) {
                let urlStr = url[1].toString()
                if (Helpers.isCommitUrl(urlStr) && urlStr.includes(gitRepo)) {
                    offence = true;
                    break;
                }
            }
        }

        return [
            !offence,
            `Please use the commit hash instead of the commit full URL.`
                + Helpers.errMessageSuffix
        ];
    }
}
