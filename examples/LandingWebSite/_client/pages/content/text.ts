import { Textbox, TextboxOptions } from "../form/textbox";
import { IContentField, IContentForm } from "../typings/content";

export class TextContent extends Textbox implements IContentField {
    readonly form: IContentForm;

    constructor(form: IContentForm, name: string, options: TextboxOptions) {
        super(name, options);

        this.form = form;
    }

    protected _onChanged() {
        super._onChanged();

        this.form.queue.request({
            url: '/brandup.pages/content/text',
            urlParams: {
                editId: this.form.editId,
                path: this.form.contentPath,
                field: this.name
            },
            method: "POST",
            type: "JSON",
            data: this.getValue(),
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