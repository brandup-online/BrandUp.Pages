import { FieldProvider } from "./base";
import { HtmlDesigner } from "../designer/html";
import { AjaxResponse } from "@brandup/ui-ajax";
import { FieldValueResult } from "../../typings/content";

export class HtmlFieldProvider extends FieldProvider<string, HtmlFieldOptions> {
    readonly isTranslatable: boolean = true;

    createDesigner() {
        return new HtmlDesigner(this);
    }

    async saveValue(value: string) {
        const response: AjaxResponse<FieldValueResult> = await this.request({
            url: '/brandup.pages/content/html',
            method: "POST",
            type: "JSON",
            data: value ? value : "",
        });
        
        if (response.status === 200) {
            if (!response.data) throw new Error("error load data");

            this.onSavedValue(response.data);

            if (this.valueElem) {
                let value = this.getValue();
                this.valueElem.innerHTML = value ? value : "";
            }
        }
    }
}

export interface HtmlFieldOptions {
    placeholder: string;
}