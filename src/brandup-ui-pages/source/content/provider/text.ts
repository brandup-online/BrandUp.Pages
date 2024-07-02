import { DesignerEvent } from "../../content/designer/base";
import { TextboxOptions } from "../../form/textbox";
import { TextDesigner } from "../designer/text";
import { FieldProvider } from "./base";

export class TextFieldProvider extends FieldProvider<string, TextboxOptions> {
    createDesigner() {
        return new TextDesigner(this.__editor, this.__valueElem, this.__model.options || {});
    }

    protected _onChange(e: DesignerEvent<string>): void {
        this.request({
            url: '/brandup.pages/content/text',
            method: "POST",
            type: "JSON",
            data: e.value ? e.value : "",
            success: (response) => {
                if (response.status === 200) {
                    this.setValue(response.data.value);
                    this.designer?.setValid(response.data.errors.length === 0);
                    this.field?.setErrors(response.data.errors);
                }
            }
        });
    }
}