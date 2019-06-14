import { Textbox, TextboxOptions } from "../../form/textbox";
import { IContentField, IContentForm } from "../../typings/content";

export class TextContent extends Textbox implements IContentField {
    readonly form: IContentForm;

    constructor(form: IContentForm, name: string, options: TextboxOptions) {
        super(name, options);

        this.form = form;
    }

    protected _onChanged() {
        super._onChanged();

        var value = this.getValue();

        this.form.request(this, {
            url: '/brandup.pages/content/text',
            method: "POST",
            type: "JSON",
            data: value ? value : "",
            success: (data: string, status: number) => {
                if (status === 200) {
                    this.setValue(data);
                }
                else {
                    this.setErrors([ "error" ]);
                }
            }
        });
    }
}