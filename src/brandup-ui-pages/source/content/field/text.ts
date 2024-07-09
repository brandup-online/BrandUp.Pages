import { DOM } from "brandup-ui-dom";
import { TextFieldProvider } from "../../content/provider/text";
import { Textbox, TextboxOptions } from "../../form/textbox";
import { FormField } from "./base";

export class TextContent extends FormField<TextboxOptions> {
    private __isChanged: boolean;

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element.classList.add("text");
    }

    protected _renderValueElem() {
        const valueElem = DOM.tag("div", { class: "value", "tabindex": 0, contenteditable: true }) as HTMLInputElement;

        const placeholderElem = DOM.tag("div", { class: "placeholder" }, this.options.placeholder);
        placeholderElem.addEventListener("click", () => {
            valueElem.focus();
        });
        valueElem.appendChild(placeholderElem);

        valueElem.addEventListener("paste", (e: ClipboardEvent) => {
            this.__isChanged = true;

            e.preventDefault();

            const text = e.clipboardData.getData("text/plain");
            document.execCommand("insertText", false, this.normalizeValue(text));
        });
        valueElem.addEventListener("cut", () => {
            this.__isChanged = true;
        });
        valueElem.addEventListener("keydown", (e: KeyboardEvent) => {
            if (!this.options.allowMultiline && e.keyCode === 13) {
                e.preventDefault();
                return false;
            }
        });
        valueElem.addEventListener("keyup", () => {
            this.__isChanged = true;
        });
        valueElem.addEventListener("focus", () => {
            this.__isChanged = false;
            valueElem.classList.add("focused");
        });
        valueElem.addEventListener("blur", () => {
            valueElem.classList.remove("focused");
            if (this.__isChanged)
                this._onChanged();
        });

        return valueElem;
    }

    protected _setValue(value: string) {
        value = this.normalizeValue(value);
        if (value && this.options.allowMultiline) {
            value = value.replace(/(?:\r\n|\r|\n)/g, "<br />");
        }
        this.__valueElem.innerHTML = value ? value : "";
    }

    getValue(): string {
        const val = this.normalizeValue(this.__valueElem.innerText);
        return val ? val : null;
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