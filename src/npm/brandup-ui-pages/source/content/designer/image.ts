import { FieldDesigner } from "./base";
import { ImageFieldProvider } from "../provider/image";
import { DOM } from "@brandup/ui-dom";
import iconUpload from "../../svg/toolbar-button-picture.svg";
import "./image.less";

interface IImageDesignerElements {
    fileInputElem: HTMLInputElement;
    menuElem: HTMLElement;
    progressElem: HTMLElement;
    button: HTMLElement;
    textInput: HTMLInputElement;
}

export class ImageDesigner extends FieldDesigner<ImageFieldProvider> {
    private __elements?: IImageDesignerElements;
    private __closeFunc: (e: MouseEvent) => void = () => {};

    get elements(): IImageDesignerElements {
        if (!this.__elements) throw "Image designer elements is not set";
        return this.__elements;
    }

    get typeName(): string { return "BrandUpPages.ImageDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("image-designer");

        const button = DOM.tag("button", { title: "Управление картинкой" }, iconUpload);
        const textInput = DOM.tag("input", { type: "text" }) as HTMLInputElement;
        const menuElem = DOM.tag("div", { class: "bp-elem image-designer-menu" }, [
            DOM.tag("ul", { class: "field-designer-popup" }, [
                //DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "open-editor" }, "Редактор")),
                //DOM.tag("li", { class: "split" }),
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "upload-file" }, "Загрузить с компьютера"))
            ]),
            button, textInput
        ])
        
        elem.appendChild(menuElem);

        const fileInputElem = DOM.tag("input", { type: "file" }) as HTMLInputElement;

        const progressElem = DOM.tag("div", { class: "bp-elem image-designer-progress" })
        elem.appendChild(progressElem);

        this.__elements = { button, textInput, menuElem, fileInputElem, progressElem };

        this.elements.fileInputElem.addEventListener("change", () => {
            if (!this.elements.fileInputElem.files || this.elements.fileInputElem.files.length === 0)
                return;

            this.__uploadFile(this.elements.fileInputElem.files.item(0)!);

            this.elements.textInput.focus();
        });

        this.elements.textInput.addEventListener("paste", (e: ClipboardEvent) => {
            e.preventDefault();

            if (!e.clipboardData) return;

            if (e.clipboardData.files.length > 0) {
                this.__uploadFile(e.clipboardData.files.item(0)!);
            }
            else if (e.clipboardData.types.indexOf("text/plain") >= 0) {
                const url = e.clipboardData.getData("text");
                if (url && url.toLocaleLowerCase().startsWith("http"))
                    this.__uploadFile(url);
            }

            this.elements.textInput.focus();

            elem.classList.remove("opened-menu");
            document.body.removeEventListener("click", this.__closeFunc, false);
        });

        this.__closeFunc = (e: MouseEvent) => {
            const t = e.target as Element;
            if (!t.closest(".image-designer-menu"))
                elem.classList.remove("opened-menu");
        };

        this.elements.button.addEventListener("click", (e: MouseEvent) => {
            e.preventDefault();
            e.stopImmediatePropagation();

            if (!elem.classList.toggle("opened-menu")) {
                document.body.removeEventListener("click", this.__closeFunc, false);
                return;
            }

            document.body.addEventListener("click", this.__closeFunc, false);
            this.elements.textInput.focus();
        });

        this.registerCommand("upload-file", () => {
            elem.classList.remove("opened-menu");
            document.body.removeEventListener("click", this.__closeFunc, false);

            if (!this.elements.fileInputElem) throw new Error("File uploading error");
            this.elements.fileInputElem.click();
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

            if (!e.dataTransfer) return false;
            const file = e.dataTransfer.files.item(0);
            if (!file?.type)
                return false;

            this.__uploadFile(file);

            if (e.dataTransfer.items)
                e.dataTransfer.items.clear();
            else
                e.dataTransfer.clearData();

            this.elements.textInput.focus();

            return false;
        };
    }
    
    private __uploadFile(file: File | string) {
        this.element?.classList.add("uploading");
        this.provider.saveValue(file);
    }

    destroy() {
        this.element?.classList.remove("image-designer");

        super.destroy();
    }
}