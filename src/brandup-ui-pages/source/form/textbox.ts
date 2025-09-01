import { Field } from "./field";
import { DOM } from "@brandup/ui-dom";
import "./textbox.less";

export class Textbox extends Field<string, TextboxOptions> {
    private __valueElem?: HTMLElement;
    private __isChanged: boolean = false;

    get typeName(): string { return "BrandUpPages.Form.Field.Text"; }

    protected override _onRender() {
        super._onRender();

        if (!this.element)
            return;

        this.element.classList.add("text");

        this.__valueElem = DOM.tag("div", { class: "value", "tabindex": 0, contenteditable: true }) as HTMLInputElement;
        this.element.appendChild(this.__valueElem);

        const placeholderElem = DOM.tag("div", { class: "placeholder" }, this.options.placeholder);
        placeholderElem.addEventListener("click", () => {
            this.__valueElem?.focus();
        });
        this.element.appendChild(placeholderElem);

        this.__valueElem.addEventListener("paste", (e: ClipboardEvent) => {
            this.__isChanged = true;

            e.preventDefault();

            const text = e.clipboardData?.getData("text/plain");
            if (!text)
                return;
            document.execCommand("insertText", false, this.normalizeValue(text));
        });
        this.__valueElem.addEventListener("cut", () => {
            this.__isChanged = true;
        });
        this.__valueElem.addEventListener("keydown", (e: KeyboardEvent) => {
            if (!this.options.allowMultiline && e.keyCode === 13) {
                e.preventDefault();
                return false;
            }

            return true;
        });
        this.__valueElem.addEventListener("keyup", () => {
            this.__isChanged = true;
        });
        this.__valueElem.addEventListener("focus", () => {
            this.__isChanged = false;
            this.element?.classList.add("focused");
        });
        this.__valueElem.addEventListener("blur", () => {
            this.element?.classList.remove("focused");
            if (this.__isChanged)
                this._onChanged();
        });
    }

    private __refreshUI() {
        const hasVal = this.hasValue();
        if (hasVal)
            this.element?.classList.add("has-value");
        else
            this.element?.classList.remove("has-value");
    }
    protected _onChanged() {
        this.__refreshUI();

        this.raiseChanged();
    }

    getValue(): string | null {
        if (!this.__valueElem)
            return null;
        const val = this.normalizeValue(this.__valueElem.innerText);
        return val ? val : null;
    }
    setValue(value: string) {
        value = this.normalizeValue(value);
        if (value && this.options.allowMultiline) {
            value = value.replace(/(?:\r\n|\r|\n)/g, "<br />");
        }
        if (this.__valueElem)
            this.__valueElem.innerHTML = value ? value : "";

        this.__refreshUI();
    }
    hasValue(): boolean {
        if (!this.__valueElem)
            return false;

        const val = this.normalizeValue(this.__valueElem.innerText);
        return val ? true : false;
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
export interface TextboxOptions {
    placeholder?: string;
    allowMultiline?: boolean;
}