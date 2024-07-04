import { DOM } from "brandup-ui-dom";
import "./image.less";
import { FormField } from "./base";
import { ImageFieldProvider } from "../../content/provider/image";
import { IContentField } from "../provider/base";

export class ImageContent extends FormField<ImageFieldValue, ImageFieldOptions, ImageFieldProvider> implements IContentField {
    private valueElem: HTMLElement;
    private __isChanged = false;
    private __fileInputElem: HTMLInputElement;
    private __value: ImageFieldValue = null;

    get typeName(): string { return "BrandUpPages.Form.Field.Image"; }

    protected _onRender() {
        super._onRender();

        this.element.classList.add("image");

        this.valueElem = DOM.tag("div", { class: "value", "tabindex": 0 }, [
            DOM.tag("span", null, "Выберите или перетащите суда файл с изображением"),
            DOM.tag("button", { "data-command": "select-file" }, [
                "Выбрать файл"
            ]),
            this.__fileInputElem = DOM.tag("input", { type: "file" }) as HTMLInputElement
        ]);
        this.element.appendChild(this.valueElem);

        this.__fileInputElem.addEventListener("change", () => {
            if (this.__fileInputElem.files.length === 0)
                return;

            this.__uploadFile(this.__fileInputElem.files.item(0));

            this.valueElem.focus();
        });

        let dragleaveTime = 0;
        this.valueElem.ondragover = () => {
            clearTimeout(dragleaveTime);
            this.valueElem.classList.add("draging");
            return false;
        };
        this.valueElem.ondragleave = () => {
            dragleaveTime = window.setTimeout(() => { this.valueElem.classList.remove("draging"); }, 50);
            return false;
        };
        this.valueElem.ondrop = (e: DragEvent) => {
            e.stopPropagation();
            e.preventDefault();

            this.valueElem.classList.remove("draging");

            const file = e.dataTransfer.files.item(0);
            if (!file.type)
                return false;

            this.__uploadFile(file);

            if (e.dataTransfer.items)
                e.dataTransfer.items.clear();
            else
                e.dataTransfer.clearData();

            this.valueElem.focus();

            return false;
        };

        this.valueElem.addEventListener("paste", (e: ClipboardEvent) => {
            e.preventDefault();

            if (e.clipboardData.files.length > 0) {
                this.__uploadFile(e.clipboardData.files.item(0));
            }
            else if (e.clipboardData.types.indexOf("text/plain") >= 0) {
                const url = e.clipboardData.getData("text");
                if (url && url.toLocaleLowerCase().startsWith("http"))
                    this.__uploadFile(url);
            }

            this.valueElem.blur();
        });

        this.registerCommand("select-file", () => {
            this.__fileInputElem.click();
        });
    }
    private __uploadFile(file: File | string) {
        this.provider.changeImage(file);
    }

    private __refreshValueUI() {
        this.valueElem.style.backgroundImage = this.__value ? "url(" + this.__value.previewUrl + ")" : "none";

        if (this.__value)
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    }

    getValue(): ImageFieldValue {
        return this.__value;
    }
    setValue(value: ImageFieldValue) {
        this.__value = value;

        this.__refreshValueUI();
    }
    hasValue(): boolean {
        return this.__value ? true : false;
    }
}

export interface ImageFieldOptions {
}
export interface ImageFieldValue {
    valueType: string;
    value: string;
    previewUrl: string;
}