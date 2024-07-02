import { TextFieldProvider } from "../../content/provider/text";
import { Textbox, TextboxOptions } from "../../form/textbox";
import { IContentField, IContentForm } from "../../typings/content";

export class TextContent extends Textbox implements IContentField {
    readonly provider: TextFieldProvider;

    constructor(name: string, errors: string[], options: TextboxOptions, provider: TextFieldProvider,) {
        super(name, errors, options);
        this.provider = provider;
    }

    protected _onChanged() {
        super._onChanged();
        const value = this.getValue();
        this.provider.setValue(value);
    }
}