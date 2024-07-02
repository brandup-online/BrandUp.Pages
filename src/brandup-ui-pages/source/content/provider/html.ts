import { FieldProvider } from "./base";
import { HtmlDesigner, HtmlFieldFormOptions } from "../designer/html";
import { AjaxResponse } from "brandup-ui-ajax";

export class HtmlFieldProvider extends FieldProvider<string, HtmlFieldFormOptions> {
    createDesigner() {
        return new HtmlDesigner(this.__valueElem, this.__model.options, this);
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