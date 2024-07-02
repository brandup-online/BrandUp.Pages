import { DesignerEvent } from "../../content/designer/base";
import { ImageFieldValue, ImageFieldOptions } from "../../content/field/image";
import { ImageDesigner } from "../designer/image";
import { FieldProvider } from "./base";
import { AjaxResponse } from "brandup-ui-ajax";

export class ImageFieldProvider extends FieldProvider<ImageFieldValue, ImageFieldOptions> {
    createDesigner() {
        return new ImageDesigner(this.__editor, this.__valueElem, this.__model.options);
    }

    protected _onChange(e: DesignerEvent<File | string>): void {
        e.target.classList.add("uploading");
        const width = e.target.getAttribute("content-image-width");
        const height = e.target.getAttribute("content-image-height");

        let requestOptions: {[key: string]: any} = {};
        if (e.value instanceof File) {
            requestOptions = {
                url: `/brandup.pages/content/image`,
                urlParams: {
                    fileName: e.value.name,
                    width: width,
                    height: height
                },
                data: e.value,
            }
        }
        else if (typeof e.value === "string") {
            requestOptions = {
                url: `/brandup.pages/content/image/url`,
                urlParams: {
                    url: e.value,
                    width: width,
                    height: height
                },
            }
        }
        else return e.target.classList.remove("uploading");

        this.request({
            ...requestOptions, 
            method: "POST",
            success: (response: AjaxResponse) => {
                switch (response.status) {
                    case 200:
                        this.setValue(response.data.value);
                        this.designer?.setValid(response.data.errors.length === 0);
                        this.field?.setErrors(response.data.errors);
                        break;
                }

                e.target.classList.remove("uploading");
            }
        });
    }
}