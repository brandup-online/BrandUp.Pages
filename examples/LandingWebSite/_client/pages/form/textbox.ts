import { Field } from "./field";
import { DOM } from "brandup-ui";
import "./textbox.less";

export class TextField extends Field<string, TextFieldOptions> {
    private __valueElem: HTMLElement;
    private __isChanged: boolean;

    get typeName(): string { return "BrandUpPages.Form.Field.Text"; }

    protected _onRender() {
        super._onRender();

        this.element.classList.add("text");

        this.__valueElem = <HTMLInputElement>DOM.tag("div", { class: "value", "tabindex": 0, contenteditable: true });
        this.element.appendChild(this.__valueElem);

        let placeholderElem = DOM.tag("div", { class: "placeholder" }, this.options.placeholder);
        placeholderElem.addEventListener("click", () => {
            this.__valueElem.focus();
        });
        this.element.appendChild(placeholderElem);

        this.__valueElem.addEventListener("paste", (e: ClipboardEvent) => {
            this.__isChanged = true;

            e.preventDefault();

            var text = e.clipboardData.getData("text/plain");
            document.execCommand("insertText", false, this.normalizeValue(text));
        });
        this.__valueElem.addEventListener("cut", () => {
            this.__isChanged = true;
        });
        this.__valueElem.addEventListener("keydown", (e: KeyboardEvent) => {
            if (!this.options.allowMultiline && e.keyCode == 13) {
                e.preventDefault();
                return false;
            }
        });
        this.__valueElem.addEventListener("keyup", (e: KeyboardEvent) => {
            this.__isChanged = true;
        });
        this.__valueElem.addEventListener("focus", () => {
            this.__isChanged = false;
            this.element.classList.add("focused");
        });
        this.__valueElem.addEventListener("blur", () => {
            this.element.classList.remove("focused");
            if (this.__isChanged)
                this.__onChanged();
        });
    }

    private __refreshUI() {
        let hasVal = this.hasValue();
        if (hasVal)
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    }
    private __onChanged() {
        this.__refreshUI();

        this.raiseChanged();
    }

    getValue(): string {
        var val = this.normalizeValue(this.__valueElem.innerText);
        return val ? val : null;
    }
    setValue(value: string) {
        value = this.normalizeValue(value);
        if (value && this.options.allowMultiline) {
            value = value.replace(/(?:\r\n|\r|\n)/g, "<br />");
        }
        this.__valueElem.innerHTML = value ? value : "";

        this.__refreshUI();
    }
    hasValue(): boolean {
        var val = this.normalizeValue(this.__valueElem.innerText);
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
export interface TextFieldOptions {
    placeholder?: string;
    allowMultiline?: boolean;
}