import { FieldProvider } from "./base";
import { HtmlDesigner } from "../designer/html";
import { AjaxResponse } from "brandup-ui-ajax";
import { FieldValueResult } from "../../typings/content";

export class HtmlFieldProvider extends FieldProvider<string, HtmlFieldOptions> {
    createDesigner() {
        return new HtmlDesigner(this);
    }

    createField() {
        //const { name, errors, options } = this.model;
        //this.field = new HtmlContent(name, errors, options, this);
        //return this.field;

        throw "";
    }

    saveValue(value: string): void {
        this.request({
            url: '/brandup.pages/content/html',
            method: "POST",
            type: "JSON",
            data: value ? value : "",
            success: (response: AjaxResponse<FieldValueResult>) => {
                if (response.status === 200) {
                    this.onSavedValue(response.data);

                    let value = this.getValue();
                    this.valueElem.innerHTML = value ? value : "";
                }
            }
        });
    }
}

export interface HtmlFieldOptions {
    placeholder: string;
}