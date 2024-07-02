import { FieldDesigner } from "./base";
import { DOM } from "brandup-ui-dom";
import "./image.less";
import iconUpload from "../../svg/toolbar-button-picture.svg";
import { ImageFieldOptions, ImageFieldValue } from "../field/image";
import { AjaxResponse } from "brandup-ui-ajax";
import { ImageFieldProvider } from "../../content/provider/image";

export class ImageDesigner extends FieldDesigner<ImageFieldOptions, ImageFieldProvider> {
    private __fileInputElem: HTMLInputElement;
    private __menuElem: HTMLElement;
    private __progressElem: HTMLElement;
    private __button: HTMLElement;
    private __closeFunc: (e: MouseEvent) => void;

    get typeName(): string { return "BrandUpPages.ImageDesigner"; }
    protected onRender(elem: HTMLElement) {
        elem.classList.add("image-designer");

        let textInput: HTMLInputElement;
        elem.appendChild(this.__menuElem = DOM.tag("div", { class: "bp-elem image-designer-menu" }, [
            DOM.tag("ul", { class: "field-designer-popup" }, [
                //DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "open-editor" }, "Редактор")),
                //DOM.tag("li", { class: "split" }),
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "upload-file" }, "Загрузить с компьютера"))
            ]),
            this.__button = DOM.tag("button", { title: "Управление картинкой" }, iconUpload),
            textInput = DOM.tag("input", { type: "text" }) as HTMLInputElement
        ]));

        elem.appendChild(this.__progressElem = DOM.tag("div", { class: "bp-elem image-designer-progress" }));

        this.__fileInputElem = DOM.tag("input", { type: "file" }) as HTMLInputElement;
        this.__fileInputElem.addEventListener("change", () => {
            if (this.__fileInputElem.files.length === 0)
                return;

            this.__uploadFile(this.__fileInputElem.files.item(0));

            textInput.focus();
        });

        textInput.addEventListener("paste", (e: ClipboardEvent) => {
            e.preventDefault();

            if (e.clipboardData.files.length > 0) {
                this.__uploadFile(e.clipboardData.files.item(0));
            }
            else if (e.clipboardData.types.indexOf("text/plain") >= 0) {
                const url = e.clipboardData.getData("text");
                if (url && url.toLocaleLowerCase().startsWith("http"))
                    this.__uploadFile(url);
            }

            textInput.focus();

            elem.classList.remove("opened-menu");
            document.body.removeEventListener("click", this.__closeFunc, false);
        });

        this.__closeFunc = (e: MouseEvent) => {
            const t = e.target as Element;
            if (!t.closest(".image-designer-menu"))
                elem.classList.remove("opened-menu");
        };

        this.__button.addEventListener("click", (e: MouseEvent) => {
            e.preventDefault();
            e.stopImmediatePropagation();

            if (!elem.classList.toggle("opened-menu")) {
                document.body.removeEventListener("click", this.__closeFunc, false);
                return;
            }

            document.body.addEventListener("click", this.__closeFunc, false);
            textInput.focus();
        });

        this.registerCommand("upload-file", () => {
            elem.classList.remove("opened-menu");
            document.body.removeEventListener("click", this.__closeFunc, false);
            this.__fileInputElem.click();
        });

        let dragleaveTime = 0;
        elem.ondragover = () => {
            clearTimeout(dragleaveTime);
            elem.classList.add("draging");
            return false;
        };
        elem.ondragleave = () => {
            dragleaveTime = window.setTimeout(() => { elem.classList.remove("draging"); }, 50);
            return false;
        };
        elem.ondrop = (e: DragEvent) => {
            e.stopPropagation();
            e.preventDefault();

            elem.classList.remove("draging");

            const file = e.dataTransfer.files.item(0);
            if (!file.type)
                return false;

            this.__uploadFile(file);

            if (e.dataTransfer.items)
                e.dataTransfer.items.clear();
            else
                e.dataTransfer.clearData();

            textInput.focus();

            return false;
        };
    }

    getValue() {
        return this.element.style.backgroundImage.substring(4).substring(-1);
    };

    setValue(value: ImageFieldValue) {
        this.element.style.backgroundImage = `url(${value.previewUrl})`;
        this.element.classList.remove("uploading");
    };

    hasValue(): boolean {
        return false;
    }

    private __uploadFile(file: File | string) {
        this.element.classList.add("uploading");
        const width = this.element.getAttribute("content-image-width");
        const height = this.element.getAttribute("content-image-height");
        this.provider.changeValue(file);
    }

    destroy() {
        this.element.classList.remove("image-designer");

        this.__menuElem?.remove();
        this.__fileInputElem?.remove();
        this.__progressElem?.remove();

        super.destroy();
    }
}