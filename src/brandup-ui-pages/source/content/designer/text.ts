import { FieldDesigner } from "./base";
import { TextboxOptions } from "../../form/textbox";
import "./text.less";

export class TextDesigner extends FieldDesigner<TextboxOptions> {
    private __isChanged: boolean;

    get typeName(): string { return "BrandUpPages.TextDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("text-designer");
        elem.setAttribute("tabindex", "0");
        elem.contentEditable = "true";
        if (this.options.placeholder)
            elem.setAttribute("data-placeholder", this.options.placeholder);

        elem.addEventListener("paste", (e: ClipboardEvent) => {
            this.__isChanged = true;

            e.preventDefault();

            const text = e.clipboardData.getData("text/plain");
            document.execCommand("insertText", false, this.normalizeValue(text));
        });
        elem.addEventListener("cut", () => {
            this.__isChanged = true;
        });
        elem.addEventListener("keydown", (e: KeyboardEvent) => {
            if (!this.options.allowMultiline && e.keyCode === 13) {
                e.preventDefault();
                return false;
            }

            this.__refreshUI();

            return true;
        });
        elem.addEventListener("keyup", () => {
            this.__isChanged = true;
        });
        elem.addEventListener("focus", () => {
            this.__isChanged = false;

            this.page.accentField(this);
        });
        elem.addEventListener("blur", () => {
            if (this.__isChanged)
                this._onChanged();

            this.page.clearAccent();
        });

        elem.addEventListener("click", (e: MouseEvent) => {
            e.preventDefault();
            e.stopPropagation();
        });

        this.__refreshUI();
    }

    getValue(): string {
        const val = this.normalizeValue(this.element.innerText);
        return val ? val : null;
    }
    setValue(value: string) {
        value = this.normalizeValue(value);
        if (value && this.options.allowMultiline)
            value = value.replace(/(?:\r\n|\r|\n)/g, "<br />");

        this.element.innerHTML = value ? value : "";

        this.__refreshUI();
    }
    hasValue(): boolean {
        const val = this.normalizeValue(this.element.innerText);
        return val ? true : false;
    }

    protected _onChanged() {
        this.__refreshUI();
        const value = this.getValue();

        this.request({
            url: '/brandup.pages/content/text',
            method: "POST",
            type: "JSON",
            data: value ? value : "",
            success: (response) => {
                if (response.status === 200) {
                    this.setValue(response.data);
                }
            }
        });
    }

    private __refreshUI() {
        if (this.hasValue())
            this.element.classList.remove("empty-value");
        else {
            this.element.classList.add("empty-value");
        }
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