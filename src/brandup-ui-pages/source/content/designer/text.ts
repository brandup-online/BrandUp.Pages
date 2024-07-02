import { FieldDesigner } from "./base";
import { TextboxOptions } from "../../form/textbox";
import "./text.less";

export class TextDesigner extends FieldDesigner<TextboxOptions> {
    private __isChanged: boolean;
    private __onPaste: (e: ClipboardEvent) => void;
    private __onCut: () => void;
    private __onKeyDown: (e: KeyboardEvent) => void;
    private __onKeyUp: (e: KeyboardEvent) => void;
    private __onFocus: () => void;
    private __onBlur: () => void;
    private __onClick: (e: MouseEvent) => void;

    get typeName(): string { return "BrandUpPages.TextDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("text-designer");
        elem.setAttribute("tabindex", "0");
        elem.contentEditable = "true";
        if (this.options.placeholder)
            elem.setAttribute("data-placeholder", this.options.placeholder);

        this.__onPaste = (e: ClipboardEvent) => {
            this.__isChanged = true;

            e.preventDefault();

            const text = e.clipboardData.getData("text/plain");
            document.execCommand("insertText", false, this.normalizeValue(text));
        };

        this.__onCut = () => { this.__isChanged = true; };

        this.__onKeyDown = (e: KeyboardEvent) => {
            if (!this.options.allowMultiline && e.keyCode === 13) {
                e.preventDefault();
                return false;
            }

            this.__refreshUI();
        };

        this.__onKeyUp = (e: KeyboardEvent) => {
            this.__isChanged = true;
        };

        this.__onFocus = () => {
            this.__isChanged = false;

            this.page.accentField(this);
        };

        this.__onBlur = () => {
            if (this.__isChanged)
                this._onChanged();

            this.page.clearAccent();
        };

        this.__onClick = (e: MouseEvent) => {
            e.preventDefault();
            e.stopPropagation();
        };

        elem.addEventListener("paste", this.__onPaste);
        elem.addEventListener("cut", this.__onCut);
        elem.addEventListener("keydown", this.__onKeyDown);
        elem.addEventListener("keyup", this.__onKeyUp);
        elem.addEventListener("focus", this.__onFocus);
        elem.addEventListener("blur", this.__onBlur);
        elem.addEventListener("click", this.__onClick);

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
        super._onChanged();
        this.__refreshUI();
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

    destroy() {
        this.element.classList.remove("text-designer");
        this.element.removeAttribute("tabindex");
        this.element.contentEditable = "false";
        this.element.removeAttribute("data-placeholder");
        
        this.element.removeEventListener("paste", this.__onPaste);
        this.element.removeEventListener("cut", this.__onCut);
        this.element.removeEventListener("keydown", this.__onKeyDown);
        this.element.removeEventListener("keyup", this.__onKeyUp);
        this.element.removeEventListener("focus", this.__onFocus);
        this.element.removeEventListener("blur", this.__onBlur);
        this.element.removeEventListener("click", this.__onClick);

        super.destroy();
    }
}