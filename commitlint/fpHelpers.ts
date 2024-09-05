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
}
export class Some<T> {
    value: T;

    constructor(val: T) {
        this.value = val;
    }

    public IsNone(): boolean {
        return false;
    }
    public IsSome(): boolean {
        return true;
    }
}

export type Option<T> = (None | Some<T>) & IOption;
