import { FieldDesigner } from "./base";
import { TextFieldProvider } from "../../content/provider/text";
import "./text.less";

export class TextDesigner extends FieldDesigner<TextFieldProvider> {
    private __isChanged: boolean = false;
    private __onPaste: (e: ClipboardEvent) => void;
    private __onCut: () => void;
    private __onKeyDown: (e: KeyboardEvent) => void;
    private __onKeyUp: (e: KeyboardEvent) => void;
    private __onFocus: () => void;
    private __onBlur: () => void;
    private __onClick: (e: MouseEvent) => void;

    get typeName(): string { return "BrandUpPages.TextDesigner"; }

    constructor(provider: TextFieldProvider) {
        super(provider);

        const elem = provider.valueElem;

        if (!elem) throw "the provider valueElem does not exist";

        this.__onKeyUp = (e: KeyboardEvent) => this.__isChanged = true;

        this.__onFocus = () => this.__isChanged = false;

        this.__onBlur = () => {
            if (this.__isChanged) {
                this.provider.saveValue(this.element?.innerText || "");
                this.__refreshUI();
            }
        };

        this.__onClick = (e: MouseEvent) => {
            e.preventDefault();
            e.stopPropagation();
        };

        this.__onPaste = (e: ClipboardEvent) => {
            this.__isChanged = true;

            e.preventDefault();

            const text = e.clipboardData?.getData("text/plain") || "";
            document.execCommand("insertText", false, this.provider.normalizeValue(text));
        };

        this.__onCut = () => this.__isChanged = true;

        this.__onKeyDown = (e: KeyboardEvent) => {
            if (!this.provider.options.allowMultiline && e.keyCode === 13) {
                e.preventDefault();
                return false;
            }

            this.__refreshUI();
        };

        elem.addEventListener("paste", this.__onPaste);
        elem.addEventListener("cut", this.__onCut);
        elem.addEventListener("keydown", this.__onKeyDown);
        elem.addEventListener("keyup", this.__onKeyUp);
        elem.addEventListener("focus", this.__onFocus);
        elem.addEventListener("blur", this.__onBlur);
        elem.addEventListener("click", this.__onClick);
    }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("text-designer");
        elem.setAttribute("tabindex", "0");
        elem.contentEditable = "true";

        if (this.provider.options.placeholder)
            elem.setAttribute("data-placeholder", this.provider.options.placeholder);

        this.__refreshUI();
    }
    
    private __refreshUI() {
        if (this.provider.hasValue())
            this.element?.classList.remove("empty-value");
        else {
            this.element?.classList.add("empty-value");
        }
    }
    
    destroy() {
        if (this.element) {
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
        }

        super.destroy();
    }
}