import { TextContent } from "../../content/field/text";
import { TextboxOptions } from "../../form/textbox";
import { TextDesigner } from "../designer/text";
import { FieldProvider } from "./base";

export class TextFieldProvider extends FieldProvider<string, TextboxOptions> {
    createDesigner() {
        return new TextDesigner(this.__valueElem, this.__model.options || {}, this);
    }

    createField() {
        const { name, errors, options } = this.__model;
        this.field = new TextContent(name, errors, options, this);
        return this.field;
    }

    setValue(value: string): void {
        this.request({
            url: '/brandup.pages/content/text',
            method: "POST",
            type: "JSON",
            data: value ? value : "",
            success: (response) => {
                if (response.status === 200) {
                    super.setValue(response.data.value);
                    this.setErrors(response.data.errors);
                }
            }
        });
    }
}