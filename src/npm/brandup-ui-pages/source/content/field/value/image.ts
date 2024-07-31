import { UIElement } from "@brandup/ui";
import { DOM } from "@brandup/ui-dom";
import { ImageFieldOptions } from "../image";
import { IFieldValueElement } from "../../../typings/content";

export class ImageValue extends UIElement implements IFieldValueElement {
    private __fileInputElem: HTMLInputElement;
    private __value?: ImageFieldValue;
    private __onChange: (file: File | string) => void = () => {};

    get typeName(): string { return "BrandUpPages.Form.Field.Value.Image"; }

    constructor(options: ImageFieldOptions) {
        super();
        
        const valueElem = DOM.tag("div", { class: "form-field_value image", "tabindex": 0 }, [
            DOM.tag("span", null, "Выберите или перетащите суда файл с изображением"),
            DOM.tag("button", { "data-command": "select-file" }, [
                "Выбрать файл"
            ]),
            this.__fileInputElem = DOM.tag("input", { type: "file" }) as HTMLInputElement
        ]);
        
        this.setElement(valueElem);
        this.__initLogic();
    }

    private __initLogic() {
        this.__fileInputElem.addEventListener("change", () => {
            if (!this.__fileInputElem.files || this.__fileInputElem.files.length === 0)
                return;

            this.__onChange(this.__fileInputElem.files.item(0)!);

            this.element?.focus();
        });

        this.registerCommand("select-file", () => {
            this.__fileInputElem.click();
        });

        if (this.element) {
            let dragleaveTime = 0;
            this.element.ondragover = () => {
                clearTimeout(dragleaveTime);
                this.element?.classList.add("draging");
                return false;
            };
            this.element.ondragleave = () => {
                dragleaveTime = window.setTimeout(() => { this.element?.classList.remove("draging"); }, 50);
                return false;
            };
            this.element.ondrop = (e: DragEvent) => {
                if (!e.dataTransfer) return;

                e.stopPropagation();
                e.preventDefault();
    
                this.element?.classList.remove("draging");
    
                const file = e.dataTransfer.files.item(0);
                if (!file?.type)
                    return false;
    
                this.__onChange(file);
    
                if (e.dataTransfer.items)
                    e.dataTransfer.items.clear();
                else
                    e.dataTransfer.clearData();
    
                this.element?.focus();
    
                return false;
            };
        }


        this.element?.addEventListener("paste", (e: ClipboardEvent) => {
            e.preventDefault();

            if (!e.clipboardData) throw new Error();

            if (e.clipboardData.files.length || -1 > 0) {
                this.__onChange(e.clipboardData!.files.item(0)!);
            }
            else if (e.clipboardData.types.indexOf("text/plain") || -1 >= 0) {
                const url = e.clipboardData.getData("text");
                if (url && url.toLocaleLowerCase().startsWith("http"))
                    this.__onChange(url);
            }

            this.element?.blur();
        });
    }

    private __refreshValueUI() {
        if (!this.element) return;
        this.element.style.backgroundImage = this.__value ? "url(" + this.__value.previewUrl + ")" : "none";

        if (this.__value)
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    }

    hasValue(): boolean {
        return this.__value ? true : false;
    }

    getValue(): ImageFieldValue | undefined {
        return this.__value;
    }
    setValue(value: ImageFieldValue) {
        this.__value = value;

        this.__refreshValueUI();
    }

    onChange(handler: (file: File | string) => void) {
        this.__onChange = handler;
    }
}

export interface ImageFieldValue {
    valueType: string;
    value: string;
    previewUrl: string;
}