import { FieldProvider } from "./base";
import { ImageDesigner } from "../designer/image";
import { AjaxRequest, AjaxResponse } from "@brandup/ui-ajax";
import { FieldValueResult } from "../../typings/content";

export class ImageFieldProvider extends FieldProvider<ImageFieldValue, ImageFieldOptions> {
    createDesigner() {
        return new ImageDesigner(this);
    }

    saveValue(value: File | string): void {
        let requestOptions: AjaxRequest = {};
        if (value instanceof File) {
            requestOptions = {
                url: `/brandup.pages/content/image`,
                query: {
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
                query: {
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
            success: (response: AjaxResponse<FieldValueResult>) => {
                switch (response.status) {
                    case 200:
                        if (!response.data) break;

                        this.onSavedValue(response.data);

                        if (this.valueElem) {
                            let value = this.getValue();
                            this.valueElem.style.backgroundImage = `url(${value.previewUrl})`;
                        }

                        break;
                }
            }
        });
    }
}

export interface ImageFieldOptions {
}
export interface ImageFieldValue {
    valueType: string;
    value: string;
    previewUrl: string;
}