import { FieldDesigner } from "./field";
import { TextboxOptions } from "../form/textbox";
import "./text.less";

export class TextDesigner extends FieldDesigner<TextboxOptions> {
    private __isChanged: boolean;

    get typeName(): string { return "BrandUpPages.TextDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("text-designer");
        elem.setAttribute("tabindex", "0");
        elem.contentEditable = "true";
        
        elem.addEventListener("paste", (e: ClipboardEvent) => {
            this.__isChanged = true;

            e.preventDefault();

            var text = e.clipboardData.getData("text/plain");
            document.execCommand("insertText", false, this.normalizeValue(text));
        });
        elem.addEventListener("cut", () => {
            this.__isChanged = true;
        });
        elem.addEventListener("keydown", (e: KeyboardEvent) => {
            if (!this.options.allowMultiline && e.keyCode == 13) {
                e.preventDefault();
                return false;
            }
        });
        elem.addEventListener("keyup", (e: KeyboardEvent) => {
            this.__isChanged = true;
        });
        elem.addEventListener("focus", () => {
            this.__isChanged = false;
        });
        elem.addEventListener("blur", () => {
            if (this.__isChanged)
                this._onChanged();
        });

        this.__refreshUI();
    }

    getValue(): string {
        var val = this.normalizeValue(this.element.innerText);
        return val ? val : null;
    }
    setValue(value: string) {
        value = this.normalizeValue(value);
        if (value && this.options.allowMultiline) {
            value = value.replace(/(?:\r\n|\r|\n)/g, "<br />");
        }
        this.element.innerHTML = value ? value : "";

        this.__refreshUI();
    }
    hasValue(): boolean {
        var val = this.normalizeValue(this.element.innerText);
        return val ? true : false;
    }

    protected _onChanged() {
        this.__refreshUI();

        this.page.queue.request({
            url: '/brandup.pages/content/text',
            urlParams: {
                editId: this.page.editId,
                path: this.path,
                field: this.name
            },
            method: "POST",
            type: "JSON",
            data: this.getValue(),
            success: (data: string, status: number) => {
                if (status === 200) {
                    this.setValue(data);
                }
            }
        });
    }

    private __refreshUI() {
        if (this.hasValue())
            this.element.classList.remove("has-value");
        else
            this.element.classList.add("empty-value");
    }

    normalizeValue(value: string): string {
        if (!value)
            return "";

        value = value.trim();

        if (!this.options.allowMultiline)
            value = value.replace("\n\r", " ");

        return value;
    }
}