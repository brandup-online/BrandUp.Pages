import { Textbox, TextboxOptions } from "../../form/textbox";
import { IContentField, IContentForm } from "../../typings/content";
import { AjaxResponse } from "brandup-ui-ajax";

export class TextContent extends Textbox implements IContentField {
    readonly form: IContentForm;

    constructor(form: IContentForm, name: string, errors: string[], options: TextboxOptions) {
        super(name, errors, options);

        this.form = form;
    }

    protected _onChanged() {
        super._onChanged();

        const value = this.getValue();

        this.form.request(this, {
            url: '/brandup.pages/content/text',
            method: "POST",
            type: "JSON",
            data: value ? value : "",
            success: (response: AjaxResponse<string>) => {
                if (response.status === 200) {
                    this.setValue(response.data);
                }
                else {
                    this.setErrors([]); // TODO список ошибок с сервера
                }
            }
        });
    }
}