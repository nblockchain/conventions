interface IOption {
    /**
     * @deprecated it is better to use `if (foo instanceof None)` so that you can access the .value in the `else` case
     **/
    IsNone(): boolean;
    /**
     * @deprecated it is better to use `if (!(foo instanceof None))` so that you can access the .value inside the `if` block
     **/
    IsSome(): boolean;
}

export class None {
    public IsNone(): boolean {
        return true;
    }
    public IsSome(): boolean {
        return false;
    }

    /**
     * @deprecated it is better to use `OptionStatic.None`
     **/
    constructor() {}
}
export class Some<T> {
    value: T;

    constructor(val: NonNullable<T>) {
        this.value = val;
    }

    public IsNone(): boolean {
        return false;
    }
    public IsSome(): boolean {
        return true;
    }
}

export type Option<T> = (None | Some<NonNullable<T>>) & IOption;

export class OptionStatic {
    public static None = new None();
    public static OfObj<T>(obj: T | null | undefined): Option<NonNullable<T>> {
        if (obj === null || obj === undefined) {
            return OptionStatic.None;
        } else {
            return new Some(obj);
        }
    }
}

export class TypeHelpers {
    // because instanceof doesn't work with primitive types (e.g. String), taken from https://stackoverflow.com/a/58184883/544947
    public static IsInstanceOf(variable: any, type: any) {
        let res: boolean = false;
        if (typeof type == "string") {
            res = typeof variable == type.toLowerCase();
        } else {
            res = variable.constructor == type;
        }
        return res;
    }
}
