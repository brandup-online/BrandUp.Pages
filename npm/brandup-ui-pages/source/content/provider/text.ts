import { AjaxResponse } from "@brandup/ui-ajax";
import { TextDesigner } from "../designer/text";
import { FieldProvider } from "./base";
import { FieldValueResult } from "../../typings/content";

export class TextFieldProvider extends FieldProvider<string, TextFieldOptions> {
    createDesigner() {
        return new TextDesigner(this);
    }
    
    saveValue(value: string) {
        value = value ?? "";
        value = this.normalizeValue(value);

        this.request({
            url: '/brandup.pages/content/text',
            method: "POST",
            type: "JSON",
            data: value,
            success: (response: AjaxResponse<FieldValueResult>) => {
                if (response.status === 200) {
                    this.onSavedValue(response.data);

                    let value = this.getValue();
                    value = this.normalizeValue(value);
                    if (value && this.options.allowMultiline)
                        value = value.replace(/(?:\r\n|\r|\n)/g, "<br />");

                    this.valueElem.innerHTML = value ? value : "";
                }
            }
        });
    }

    createField() {
        //const { name, errors, options } = this.model;
        //this.field = new TextContent(name, errors, options, this);
        //return this.field;

        throw "";
    }
    
    normalizeValue(value: string): string {
        if (!value)
            return "";

        value = value.trim();

        if (!this.options.allowMultiline)
            value = value.replace("\n\r", " ");

        return value;
    }
}

export interface TextFieldOptions {
    placeholder?: string;
    allowMultiline?: boolean;
}