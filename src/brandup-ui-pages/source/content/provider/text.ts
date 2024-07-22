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
                if (response.status === 200 && response.data) {
                    this.onSavedValue(response.data);
                }
            }
        });
    }

    protected onSavedValue(model: FieldValueResult) {
        model.value = this.normalizeValue(model.value);
        super.onSavedValue(model);
    }
    
    normalizeValue(value: string): string {
        if (!value)
            return "";

        value = value.trim();

        if (!this.options.allowMultiline)
            value = value.replace(/(?:\r\n|\r|\n)/g, "<br />");

        return value;
    }
}

export interface TextFieldOptions {
    placeholder?: string;
    allowMultiline?: boolean;
}