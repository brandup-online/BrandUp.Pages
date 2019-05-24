import { FieldDesigner } from "./base";
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

        this.__button.addEventListener("click", (e: MouseEvent) => {
            e.preventDefault();
            e.stopPropagation();

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

            let width = this.element.getAttribute("content-image-width");
            let height = this.element.getAttribute("content-image-height");

            this.request({
                url: `/brandup.pages/content/image`,
                urlParams: {
                    fileName: file.name,
                    width: width,
                    height: height
                },
                method: "POST",
                data: fileObject,
                success: (data: string, status: number) => {
                    switch (status) {
                        case 200:
                            this.element.style.backgroundImage = `url(${data})`;

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