import { DOM } from "brandup-ui-dom";
import ContentEditor from "brandup-pages-ckeditor";
import "./html.less";
import { FormField } from "./base";
import { IFormField } from "../provider/base";

export class HtmlContent extends FormField<HtmlFieldFormOptions> {
    private __isChanged: boolean;
    private __editor: ContentEditor;
    private __editorPromise: Promise<any>;

    get typeName(): string { return "BrandUpPages.Form.Field.Html"; }

    protected _renderValueElem() {
        const valueElem = DOM.tag("div", { class: "value" });
        if (this.options.placeholder)
            valueElem.setAttribute("data-placeholder", this.options.placeholder);

        this.__editorPromise = ContentEditor.create(valueElem, { placeholder: this.options.placeholder })
            .then(editor => {
                this.__editor = editor;

                editor.model.document.on('change', () => {
                    if (editor.model.document.differ.hasDataChanges())
                        this.__isChanged = true;

                    this.__refreshUI();
                });

                this.__refreshUI();
            });

        valueElem.addEventListener("focus", () => {
            this.__isChanged = false;
        });
        valueElem.addEventListener("blur", () => {
            if (this.__isChanged) {
                this.__editor.model.document.differ.reset();
                this._onChanged();
            }

            this.__refreshUI();
        });

        return valueElem;
    }

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element.classList.add("html");
    }

    getValue(): string {
        const data = this.__editor.data.get();
        return data ? data : null;
    }

    protected _setValue(value: string) {
        this.__editorPromise.then (()=> { // если editor еще не создался - ждем
            if (this.__editor) {
                this.__editor.data.set(value ? value : "");
                this.__refreshUI();
            }
            else
                this.__valueElem.innerHTML = value ? value : "";
        });
    }
    hasValue(): boolean {
        const value = this.normalizeValue(this.__valueElem.innerText);
        if (!value)
            return false;

        const val = this.__editor.model.hasContent(this.__editor.model.document.getRoot(), { ignoreWhitespaces: true });
        return value && val ? true : false;
    }
    private __refreshUI() {
        if (this.hasValue())
            this.element.classList.remove("empty-value");
        else
            this.element.classList.add("empty-value");
    }

    normalizeValue(value: string): string {
        if (!value)
            return "";

        value = value.trim();

        return value;
    }

    destroy() {
        this.__editor.destroy().then(() => {
            super.destroy();
        });

        super.destroy();
    }
}

export interface HtmlFieldFormOptions {
    placeholder: string;
}