import { FieldProvider } from "./base";
import { HtmlDesigner } from "../designer/html";
import { AjaxResponse } from "@brandup/ui-ajax";
import { FieldValueResult } from "../../typings/content";

export class HtmlFieldProvider extends FieldProvider<string, HtmlFieldOptions> {
    createDesigner() {
        return new HtmlDesigner(this);
    }

    saveValue(value: string): void {
        this.request({
            url: '/brandup.pages/content/html',
            method: "POST",
            type: "JSON",
            data: value ? value : "",
            success: (response: AjaxResponse<FieldValueResult>) => {
                if (response.status === 200) {
                    if (!response.data) throw "error load data";

                    this.onSavedValue(response.data);

                    if (this.valueElem) {
                        let value = this.getValue();
                        this.valueElem.innerHTML = value ? value : "";
                    }
                }
            }
        });
    }
}

export interface HtmlFieldOptions {
    placeholder: string;
}