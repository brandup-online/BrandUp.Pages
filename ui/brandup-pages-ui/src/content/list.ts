import { Textbox, TextboxOptions } from "../form/textbox";
import { IContentField, IContentForm } from "../typings/content";

export class ListContent extends Textbox implements IContentField {
    readonly form: IContentForm;

    constructor(form: IContentForm, name: string, options: TextboxOptions) {
        super(name, options);

        this.form = form;
    }

    protected _onChanged() {
        super._onChanged();
    }
}