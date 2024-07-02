import { ImageFieldValue, ImageFieldOptions } from "../../content/field/image";
import { ImageDesigner } from "../designer/image";
import { FieldProvider } from "./base";
import { AjaxResponse } from "brandup-ui-ajax";

export class ImageFieldProvider extends FieldProvider<ImageFieldValue, ImageFieldOptions> {
    createDesigner() {
        return new ImageDesigner(this.__valueElem, this.__model.options, this);
    }

    setValue(value: File | string): void {
        let requestOptions: {[key: string]: any} = {};
        if (value instanceof File) {
            requestOptions = {
                url: `/brandup.pages/content/image`,
                urlParams: {
                    fileName: value.name,
                    // width: width,
                    // height: height
                },
                data: value,
            }
        }
        else if (typeof value === "string") {
            requestOptions = {
                url: `/brandup.pages/content/image/url`,
                urlParams: {
                    url: value,
                    // width: width,
                    // height: height
                },
            }
        }
        else return;

        this.request({
            ...requestOptions, 
            method: "POST",
            success: (response: AjaxResponse) => {
                switch (response.status) {
                    case 200:
                        super.setValue(response.data.value);
                        this.setErrors(response.data.errors);
                        break;
                }
            }
        });
    }
}