import { Field } from "../../form/field";
import { FieldProvider } from "../provider/base";

export abstract class FormField <TValue, TOptions, TProvider extends FieldProvider<TValue, TOptions>> extends Field<TValue, TOptions>  {
    readonly provider: TProvider;

    constructor(name: string, errors: string[], options: TOptions, provider: TProvider) {
        super(name, errors, options);
        this.provider = provider;
    }

    protected _onChanged() {
        this.raiseChanged();
        const value = this.getValue();
        //this.provider.setValue(value);
    }
}