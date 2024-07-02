import { Field } from "../../form/field";
import { IContentFieldProvider } from "../../typings/content";

export abstract class FormField <TValue, TOptions, TProvider extends IContentFieldProvider> extends Field<TValue, TOptions>  {
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