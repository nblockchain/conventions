export class TypeHelpers {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any -- method accepts any value by design
    public static IsNullOrUndefined(variable: any) {
        return variable === null || variable === undefined;
    }

    // because instanceof doesn't work with primitive types (e.g. String), taken from https://stackoverflow.com/a/58184883/544947
    // eslint-disable-next-line @typescript-eslint/no-explicit-any -- method accepts any value by design
    public static IsInstanceOf(variable: any, type: any) {
        if (TypeHelpers.IsNullOrUndefined(variable)) {
            throw new Error(
                "Invalid 'variable' parameter passed in: null or undefined"
            );
        }
        if (TypeHelpers.IsNullOrUndefined(type)) {
            throw new Error(
                "Invalid 'type' parameter passed in: null or undefined"
            );
        }

        let res: boolean = false;
        if (typeof type == "string") {
            res = typeof variable == type.toLowerCase();
        } else {
            res = variable.constructor == type;
        }
        return res;
    }
}
