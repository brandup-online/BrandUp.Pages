import { FieldProvider } from "./base";
import { HtmlDesigner, HtmlFieldFormOptions } from "../designer/html";
import { AjaxResponse } from "brandup-ui-ajax";
import { HtmlContent } from "../../content/field/html";

export class HtmlFieldProvider extends FieldProvider<string, HtmlFieldFormOptions> {
    createDesigner() {
        return new HtmlDesigner(this.__valueElem, this.__model.options, this);
    }

    createField() {
        const { name, errors, options } = this.__model;
        this.field = new HtmlContent(name, errors, options, this);
        return this.field;
    }

    setValue(value: string): void {
        this.request({
            url: '/brandup.pages/content/html',
            method: "POST",
            type: "JSON",
            data: value ? value : "",
            success: (response: AjaxResponse) => {
                if (response.status === 200) {
                    super.setValue(response.data.value);
                    this.setErrors(response.data.errors);
                }
            }
        });
    }
}