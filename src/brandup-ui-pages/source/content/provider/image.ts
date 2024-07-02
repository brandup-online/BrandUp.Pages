import { ImageFieldValue, ImageFieldOptions, ImageContent } from "../../content/field/image";
import { ImageDesigner } from "../designer/image";
import { FieldProvider } from "./base";
import { AjaxResponse } from "brandup-ui-ajax";

export class ImageFieldProvider extends FieldProvider<ImageFieldValue, ImageFieldOptions> {
    createDesigner() {
        return new ImageDesigner(this.__valueElem, this.__model.options, this);
    }

    createField() {
        const { name, errors, options } = this.__model;
        this.field = new ImageContent(name, errors, options, this);
        return this.field;
    }

    changeValue(value: File | string): void {
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
                        console.log("🚀 ~ ImageFieldProvider ~ changeValue ~ response.dat:", response.data)
                        super.setValue(response.data.value);
                        this.setErrors(response.data.errors);
                        break;
                }
            }
        });
    }

    setValue(value: ImageFieldValue) {
        super.setValue(value);
    }
}