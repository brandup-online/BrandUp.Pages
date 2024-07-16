import { FieldDesigner } from "./base";
import { ImageFieldProvider } from "../provider/image";
import { DOM } from "brandup-ui-dom";
import iconUpload from "../../svg/toolbar-button-picture.svg";
import "./image.less";

export class ImageDesigner extends FieldDesigner<ImageFieldProvider> {
    private __fileInputElem: HTMLInputElement = DOM.tag("input", { type: "file" }) as HTMLInputElement;
    private __menuElem: HTMLElement = this.__initMenuElem();
    private __progressElem: HTMLElement = DOM.tag("div", { class: "bp-elem image-designer-progress" });
    private __button: HTMLElement = DOM.tag("button", { title: "Управление картинкой" }, iconUpload);
    private __textInput: HTMLInputElement = DOM.tag("input", { type: "text" }) as HTMLInputElement;
    private __closeFunc: (e: MouseEvent) => void = () => {};

    get typeName(): string { return "BrandUpPages.ImageDesigner"; }

    private __initMenuElem() {
        return DOM.tag("div", { class: "bp-elem image-designer-menu" }, [
            DOM.tag("ul", { class: "field-designer-popup" }, [
                //DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "open-editor" }, "Редактор")),
                //DOM.tag("li", { class: "split" }),
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "upload-file" }, "Загрузить с компьютера"))
            ]),
            this.__button,
            this.__textInput
        ])
    }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("image-designer");

        elem.appendChild(this.__menuElem);

        elem.appendChild(this.__progressElem);

        this.__fileInputElem.addEventListener("change", () => {
            if (!this.__fileInputElem.files || this.__fileInputElem.files.length === 0)
                return;

            this.__uploadFile(this.__fileInputElem.files.item(0)!);

            this.__textInput.focus();
        });

        this.__textInput.addEventListener("paste", (e: ClipboardEvent) => {
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

            this.__textInput.focus();

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
            this.__textInput.focus();
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

            const file = e.dataTransfer?.files.item(0);
            if (!file?.type)
                return false;

            this.__uploadFile(file);

            if (!e.dataTransfer) return false;

            if (e.dataTransfer.items)
                e.dataTransfer.items.clear();
            else
                e.dataTransfer.clearData();

            this.__textInput.focus();

            return false;
        };
    }
    
    private __uploadFile(file: File | string) {
        this.element?.classList.add("uploading");
        this.provider.saveValue(file);
    }

    destroy() {
        this.element?.classList.remove("image-designer");

        this.__menuElem?.remove();
        this.__fileInputElem?.remove();
        this.__progressElem?.remove();

        super.destroy();
    }
}