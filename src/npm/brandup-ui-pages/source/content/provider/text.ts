import { AjaxResponse } from "@brandup/ui-ajax";
import { TextDesigner } from "../designer/text";
import { FieldProvider } from "./base";
import { ContentFieldModel, FieldValueResult } from "../../typings/content";
import { Content } from "../../content/content";

export class TextFieldProvider extends FieldProvider<string, TextFieldOptions> {    
    constructor(content: Content, model: ContentFieldModel) {
        super(content, model);
        this.__isTranslatable = true;
    }

    createDesigner() {
        return new TextDesigner(this);
    }
    
    async saveValue(value: string) {
        value = value ?? "";
        value = this.normalizeValue(value);

        const response: AjaxResponse<FieldValueResult> = await this.request({
            url: '/brandup.pages/content/text',
            method: "POST",
            type: "JSON",
            data: value,
        });
        if (response.status === 200 && response.data) {
            this.onSavedValue(response.data);
        }
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