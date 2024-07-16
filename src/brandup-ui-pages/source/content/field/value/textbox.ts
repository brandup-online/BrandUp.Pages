import { UIElement } from "brandup-ui";
import { DOM } from "brandup-ui-dom";
import { TextboxOptions } from "../../../form/textbox";
import { IFieldValueElement } from "../../../typings/content";

export class TextBoxValue extends UIElement implements IFieldValueElement {
    private __isChanged: boolean = false;
    private __onChange: (value: string) => void = () => {};

    readonly options: TextboxOptions;

    get typeName(): string { return "BrandUpPages.Form.Field.Value.TextBox"; }

    constructor(options: TextboxOptions) {
        super();

        this.options = options;
        
        let placeholderElem;
        const valueElem = DOM.tag("div", { class: "value text", "tabindex": 0, contenteditable: true },
            placeholderElem = DOM.tag("div", { class: "placeholder" }, options.placeholder)
        );
        placeholderElem.addEventListener("click", () => {
            valueElem.focus();
        });

        this.setElement(valueElem);

        this.__initLogic();
    }

    private __initLogic() {
        this.element?.addEventListener("paste", (e: ClipboardEvent) => {
            this.__isChanged = true;

            e.preventDefault();

            const text = e.clipboardData?.getData("text/plain");
            if (!text) throw "paste event error";
            document.execCommand("insertText", false, this.normalizeValue(text));
        });
        this.element?.addEventListener("cut", () => {
            this.__isChanged = true;
        });
        this.element?.addEventListener("keydown", (e: KeyboardEvent) => {
            if (!this.options.allowMultiline && e.keyCode === 13) {
                e.preventDefault();
                return false;
            }
        });
        this.element?.addEventListener("keyup", () => {
            this.__isChanged = true;
        });
        this.element?.addEventListener("focus", () => {
            this.__isChanged = false;
            this.element?.classList.add("focused");
        });

        this.element?.addEventListener("blur", () => {
            this.element?.classList.remove("focused");
            if (this.__isChanged)
                this.__onChange(this.getValue());
        });
    }

    onChange(handler: (value: string) => void) {
        this.__onChange = handler;
    }

    getValue(): string{
        if (!this.element) throw "element not defined";

        const val = this.normalizeValue(this.element?.innerText);
        return val;
    }

    setValue(value: string) {
        if (!this.element) throw "element not defined";

        value = this.normalizeValue(value);
        if (value && this.options.allowMultiline) {
            value = value.replace(/(?:\r\n|\r|\n)/g, "<br />");
        }
        this.element.innerHTML = value ? value : "";
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