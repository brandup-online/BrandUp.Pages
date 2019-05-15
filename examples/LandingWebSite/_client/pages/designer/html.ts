import { FieldDesigner } from "./field";
import { TextboxOptions } from "../form/textbox";
import "./html.less";

export class HtmlDesigner extends FieldDesigner<TextboxOptions> {
    private __isChanged: boolean;

    get typeName(): string { return "BrandUpPages.HtmlDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("html-designer");
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
        var val = this.normalizeValue(this.element.innerHTML);
        return val ? val : null;
    }
    setValue(value: string) {
        value = this.normalizeValue(value);

        this.element.innerHTML = value ? value : "";

        this.__refreshUI();
    }
    hasValue(): boolean {
        var val = this.normalizeValue(this.element.innerHTML);
        return val ? true : false;
    }

    protected _onChanged() {
        this.__refreshUI();

        this.request({
            url: '/brandup.pages/content/html',
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
        
        return value;
    }
}