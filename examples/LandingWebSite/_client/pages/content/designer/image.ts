﻿import { FieldDesigner } from "./base";
import { DOM } from "brandup-ui";
import "./image.less";
import iconUpload from "../../svg/upload-file.svg";
import { ImageFieldValue, ImageFieldOptions } from "../field/image";

export class ImageDesigner extends FieldDesigner<ImageFieldOptions> {
    private __fileInputElem: HTMLInputElement;
    private __button: HTMLElement;

    get typeName(): string { return "BrandUpPages.ImageDesigner"; }
    protected onRender(elem: HTMLElement) {
        elem.classList.add("image-designer");

        elem.appendChild(this.__button = DOM.tag("button", { class: "image-designer-upload", "data-command": "select-file" }, iconUpload));

        this.__fileInputElem = <HTMLInputElement>DOM.tag("input", { type: "file" })

        this.__fileInputElem.addEventListener("change", () => {
            if (this.__fileInputElem.files.length === 0)
                return;

            this.__uploadFile(this.__fileInputElem.files.item(0));
            
            this.__button.focus();
        });

        this.registerCommand("select-file", () => {
            this.__fileInputElem.click();
        });
    }

    hasValue(): boolean {
        return false;
    }

    private __uploadFile(file: File | string) {
        this.element.classList.add("uploading");

        if (file instanceof File) {
            let fileObject = <File>file;

            this.request({
                url: `/brandup.pages/content/image`,
                urlParams: {
                    fileName: file.name
                },
                method: "POST",
                data: fileObject,
                success: (data: ImageFieldValue, status: number) => {
                    switch (status) {
                        case 200:
                            this.element.style.backgroundImage = `url(${data.previewUrl})`;

                            this.element.classList.remove("uploading");

                            break;
                        default:
                            throw "";
                    }
                }
            });

            return;
        }
        else if (typeof file === "string") {
            let url = <string>file;
            console.log(url);
        }
        else {
            this.element.classList.remove("uploading");
        }
    }
}