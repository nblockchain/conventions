export abstract class Helpers {

    public static errMessageSuffix = "\nFor reference, these are the guidelines that include our commit message conventions: https://github.com/nblockchain/conventions/blob/master/WorkflowGuidelines.md";

    public static isValidUrl(text: string) {
        if (text.indexOf(" ") >= 0) {
            return false;
        }

        // Borrowed from https://www.freecodecamp.org/news/check-if-a-javascript-string-is-a-url/
        try {
            return Boolean(new URL(text));
        }
        catch (e) {
            return false;
        }
    }

    // to convert from 'any' type
    public static convertAnyToString(potentialString: any, paramName: string): string {
        if (potentialString === null || potentialString === undefined) {
            // otherwise, String(null) might give us the stupid string "null"
            throw new Error('Unexpected ' + paramName + '===null or ' + paramName + '===undefined happened');
        }
        return String(potentialString);
    }

    public static assertUrl(url: string) {
        if (!Helpers.isValidUrl(url)) {
            throw Error('This function expects a url as input')
        }
    }

    public static findUrls(text: string) {
        var urlRegex = /(https?:\/\/[^\s]+)/g;
        return text.match(urlRegex);
    }

    public static isCommitUrl(url: string) {
        Helpers.assertUrl(url);
        return url.includes('/commit/');
    }

}


