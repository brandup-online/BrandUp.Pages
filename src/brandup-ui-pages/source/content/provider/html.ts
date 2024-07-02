import { FieldProvider } from "./base";
import { HtmlDesigner, HtmlFieldFormOptions } from "../designer/html";
import { DesignerEvent } from "../../content/designer/base";
import { AjaxResponse } from "brandup-ui-ajax";

export class HtmlFieldProvider extends FieldProvider<string, HtmlFieldFormOptions> {
    createDesigner() {
        return new HtmlDesigner(this.__editor, this.__valueElem, this.__model.options);
    }

    protected _onChange(e: DesignerEvent<string>): void {
        this.request({
            url: '/brandup.pages/content/html',
            method: "POST",
            type: "JSON",
            data: e.value ? e.value : "",
            success: (response: AjaxResponse) => {
                if (response.status === 200) {
                    this.setValue(response.data.value);
                    this.designer?.setValid(response.data.errors.length === 0);
                    this.field?.setErrors(response.data.errors);
                }
            }
        });
    }
}